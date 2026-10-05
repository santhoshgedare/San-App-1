using System.Security.Claims;
using System.Text.Json;
using IdentityHub.Api.Contracts;
using IdentityHub.Application.Features.Auth.Commands.ExternalLogin;
using IdentityHub.Application.Features.Auth.Commands.ForgotPassword;
using IdentityHub.Application.Common.Models;
using IdentityHub.Application.Features.Auth.Commands.Login;
using IdentityHub.Application.Features.Auth.Commands.Logout;
using IdentityHub.Application.Features.Auth.Commands.RefreshToken;
using IdentityHub.Application.Features.Auth.Commands.Register;
using IdentityHub.Application.Features.Auth.Commands.ResetPassword;
using MediatR;
using IdentityHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;

namespace IdentityHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(ISender sender, IConfiguration configuration) : ControllerBase
{
    /// <summary>Lists the supported external login providers and whether their credentials are configured.</summary>
    [HttpGet("external/providers")]
    [AllowAnonymous]
    public ActionResult<IReadOnlyList<ExternalAuthProviderDto>> GetExternalProviders()
        => Ok(new[]
        {
            new ExternalAuthProviderDto("google", "Google", IsProviderConfigured("Google")),
            new ExternalAuthProviderDto("facebook", "Facebook (Meta)", IsProviderConfigured("Facebook"))
        });

    /// <summary>Starts Google or Facebook OAuth sign-in.</summary>
    [HttpGet("external/{provider}")]
    [AllowAnonymous]
    public IActionResult StartExternalLogin(string provider, [FromQuery] string state, [FromQuery] string clientOrigin)
    {
        if (!IsValidState(state)) return BadRequest(new { error = "Invalid sign-in state." });
        if (!TryGetAllowedOrigin(clientOrigin, out var allowedOrigin)) return BadRequest(new { error = "This client origin is not allowed." });

        var scheme = ResolveScheme(provider);
        if (scheme is null) return NotFound();
        if (!IsProviderConfigured(scheme)) return Conflict(new { error = $"{scheme} sign-in is not configured." });

        var callbackUrl = Url.ActionLink(nameof(CompleteExternalLogin), values: new
        {
            provider = provider.ToLowerInvariant(),
            state,
            clientOrigin = allowedOrigin
        });
        if (callbackUrl is null) return StatusCode(StatusCodes.Status500InternalServerError);

        var properties = new AuthenticationProperties { RedirectUri = callbackUrl };
        properties.Items["clientState"] = state;
        properties.Items["clientOrigin"] = allowedOrigin;
        return Challenge(properties, scheme);
    }

    /// <summary>Consumes the provider callback, issues the app tokens, and posts them to the validated opener origin.</summary>
    [HttpGet("external/{provider}/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> CompleteExternalLogin(
        string provider,
        [FromQuery] string state,
        [FromQuery] string clientOrigin,
        CancellationToken ct)
    {
        if (!IsValidState(state) || !TryGetAllowedOrigin(clientOrigin, out var allowedOrigin)) return BadRequest();

        var externalTicket = await HttpContext.AuthenticateAsync(ExternalAuthSchemes.Cookie);
        await HttpContext.SignOutAsync(ExternalAuthSchemes.Cookie);
        if (!externalTicket.Succeeded || externalTicket.Principal is null)
        {
            return PopupResponse(allowedOrigin, state, null, "External sign-in could not be completed.");
        }
        var callbackProperties = externalTicket.Properties?.Items;
        if (callbackProperties is null ||
            !callbackProperties.TryGetValue("clientState", out var callbackState) ||
            !callbackProperties.TryGetValue("clientOrigin", out var callbackOrigin) ||
            callbackState != state ||
            callbackOrigin != allowedOrigin)
        {
            return PopupResponse(allowedOrigin, state, null, "This sign-in request has expired. Please try again.");
        }

        var scheme = ResolveScheme(provider);
        if (scheme is null) return PopupResponse(allowedOrigin, state, null, "Unsupported sign-in provider.");

        var principal = externalTicket.Principal;
        var providerKey = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(providerKey) || string.IsNullOrWhiteSpace(email))
        {
            return PopupResponse(allowedOrigin, state, null, "The provider did not return an account ID and email address.");
        }

        var emailVerified = scheme == ExternalAuthSchemes.Google &&
            bool.TryParse(principal.FindFirstValue("email_verified"), out var isVerified) && isVerified;
        var login = await sender.Send(new ExternalLoginCommand(
            scheme,
            providerKey,
            email,
            principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
            principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty,
            emailVerified), ct);

        return login.Succeeded
            ? PopupResponse(allowedOrigin, state, login.Data, null)
            : PopupResponse(allowedOrigin, state, null, login.Errors.FirstOrDefault() ?? "External sign-in failed.");
    }

    /// <summary>Creates a new user account and returns a token pair.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResultDto>> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RegisterCommand(request.Email, request.Password, request.FirstName, request.LastName, request.PhoneNumber, AddressesController.ToInput(request.Address)), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Authenticates a user and returns a token pair.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResultDto>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new LoginCommand(request.Email, request.Password), ct);
        return result.Succeeded ? Ok(result.Data) : Unauthorized(new { errors = result.Errors });
    }

    /// <summary>Sends a one-time password reset link when the account exists.</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("password-recovery")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
    {
        var clientBaseUrl = configuration["Client:BaseUrl"] ?? "http://localhost:4200";
        var result = await sender.Send(new ForgotPasswordCommand(request.Email, clientBaseUrl), ct);
        return result.Succeeded
            ? Ok(new { message = "If an account exists for that email, a reset link will be sent." })
            : StatusCode(StatusCodes.Status503ServiceUnavailable, new { errors = result.Errors });
    }

    /// <summary>Resets the password using a one-time email token.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ResetPasswordCommand(request.Email, request.Token, request.NewPassword), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Exchanges a refresh token for a new access/refresh token pair.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResultDto>> Refresh(RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RefreshTokenCommand(request.AccessToken, request.RefreshToken), ct);
        return result.Succeeded ? Ok(result.Data) : Unauthorized(new { errors = result.Errors });
    }

    /// <summary>Revokes the given refresh token, ending the session.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken ct)
    {
        await sender.Send(new LogoutCommand(request.RefreshToken), ct);
        return NoContent();
    }

    private bool IsProviderConfigured(string provider) => provider switch
    {
        ExternalAuthSchemes.Google =>
            !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"]) &&
            !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientSecret"]),
        ExternalAuthSchemes.Facebook =>
            !string.IsNullOrWhiteSpace(configuration["Authentication:Facebook:AppId"]) &&
            !string.IsNullOrWhiteSpace(configuration["Authentication:Facebook:AppSecret"]),
        _ => false
    };

    private static string? ResolveScheme(string provider) => provider.ToLowerInvariant() switch
    {
        "google" => ExternalAuthSchemes.Google,
        "facebook" or "meta" => ExternalAuthSchemes.Facebook,
        _ => null
    };

    private bool TryGetAllowedOrigin(string value, out string origin)
    {
        origin = string.Empty;
        if (!Uri.TryCreate(value, UriKind.Absolute, out var requested) ||
            (requested.Scheme != Uri.UriSchemeHttp && requested.Scheme != Uri.UriSchemeHttps))
        {
            return false;
        }

        var canonical = requested.GetLeftPart(UriPartial.Authority);
        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"];
        if (!allowedOrigins.Any(configured => string.Equals(configured.TrimEnd('/'), canonical, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        origin = canonical;
        return true;
    }

    private static bool IsValidState(string? state) =>
        state is { Length: >= 32 and <= 128 } && state.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_');

    private IActionResult PopupResponse(string origin, string state, AuthResultDto? result, string? error)
    {
        Response.Headers.CacheControl = "no-store";
        Response.Headers.Pragma = "no-cache";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers["Content-Security-Policy"] = "default-src 'none'; script-src 'unsafe-inline'; base-uri 'none'; frame-ancestors 'none'";

        var payload = JsonSerializer.Serialize(new
        {
            type = "identityhub:external-login",
            state,
            result,
            error
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var targetOrigin = JsonSerializer.Serialize(origin);
        var html = $"<!doctype html><html><head><meta charset=\"utf-8\"><title>Sign-in complete</title></head><body><script>if(window.opener){{window.opener.postMessage({payload},{targetOrigin});window.close();}}</script><p>Sign-in complete. You may close this window.</p></body></html>";
        return Content(html, "text/html; charset=utf-8");
    }
}
