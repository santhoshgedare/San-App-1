using System.Text;
using System.Security.Claims;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Infrastructure.Identity;
using IdentityHub.Infrastructure.Email;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using IdentityHub.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace IdentityHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                // Phone-only accounts have no email, so uniqueness is enforced via UserName and in IdentityService.
                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SmtpEmailOptions>(configuration.GetSection("Email:Smtp"));
        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            var tokenLifetimeMinutes = configuration.GetValue("PasswordRecovery:TokenLifespanMinutes", 60);
            options.TokenLifespan = TimeSpan.FromMinutes(Math.Clamp(tokenLifetimeMinutes, 1, 1440));
        });
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        var authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken)
                            && context.HttpContext.Request.Path.StartsWithSegments("/hubs/order-chat"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            })
            .AddCookie(ExternalAuthSchemes.Cookie, options =>
            {
                options.Cookie.Name = "identityhub.external";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.SlidingExpiration = false;
            });

        var googleClientId = configuration["Authentication:Google:ClientId"];
        var googleClientSecret = configuration["Authentication:Google:ClientSecret"];
        if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
        {
            authenticationBuilder.AddGoogle(ExternalAuthSchemes.Google, options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
                options.SignInScheme = ExternalAuthSchemes.Cookie;
                options.CallbackPath = "/signin-google";
                options.Scope.Add("email");
                options.ClaimActions.MapJsonKey("email_verified", "email_verified");
            });
        }

        var facebookAppId = configuration["Authentication:Facebook:AppId"];
        var facebookAppSecret = configuration["Authentication:Facebook:AppSecret"];
        if (!string.IsNullOrWhiteSpace(facebookAppId) && !string.IsNullOrWhiteSpace(facebookAppSecret))
        {
            authenticationBuilder.AddFacebook(ExternalAuthSchemes.Facebook, options =>
            {
                options.AppId = facebookAppId;
                options.AppSecret = facebookAppSecret;
                options.SignInScheme = ExternalAuthSchemes.Cookie;
                options.CallbackPath = "/signin-facebook";
                options.Scope.Add("email");
                options.Scope.Add("public_profile");
                options.Fields.Add("email");
                options.Fields.Add("first_name");
                options.Fields.Add("last_name");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "first_name");
                options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "last_name");
            });
            }

        services.AddAuthorization();
        services.AddHttpContextAccessor();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<SmtpEmailSender>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();
        services.AddMemoryCache();
        services.Configure<Otp.SmsOptions>(configuration.GetSection("Sms"));
        services.AddHttpClient<Otp.ISmsSender, Otp.Msg91SmsSender>();
        services.AddScoped<IOtpService, Otp.OtpService>();
        services.AddScoped<IEmailLogService, EmailLogService>();
        services.AddScoped<IOrderChatService, OrderChatService>();
        services.AddScoped<ICustomRequestService, CustomRequestService>();
        services.AddScoped<IOrderNotifier, OrderNotifier>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IModuleAccessService, ModuleAccessService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddScoped<IApprovalWorkflowService, ApprovalWorkflowService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ISellerService, SellerService>();
        services.AddScoped<ISellerRequestService, SellerRequestService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentSettingsService, PaymentSettingsService>();
        services.AddScoped<IAddressService, AddressService>();

        return services;
    }
}
