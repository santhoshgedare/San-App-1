using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Auth.Commands.Otp;

public enum OtpPurpose
{
    Login,
    Register
}

public sealed record RequestOtpCommand(string Identifier, OtpPurpose Purpose, string? FirstName, string? LastName) : IRequest<Result>;

public sealed class RequestOtpCommandValidator : AbstractValidator<RequestOtpCommand>
{
    public RequestOtpCommandValidator()
    {
        RuleFor(x => x.Identifier).NotEmpty().MaximumLength(256);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).When(x => x.Purpose == OtpPurpose.Register);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).When(x => x.Purpose == OtpPurpose.Register);
    }
}

public sealed class RequestOtpCommandHandler(IIdentityService identityService, IOtpService otpService)
    : IRequestHandler<RequestOtpCommand, Result>
{
    public async Task<Result> Handle(RequestOtpCommand request, CancellationToken ct)
    {
        var parsed = OtpTarget.Parse(request.Identifier);
        if (!parsed.Succeeded) return Result.Failure(parsed.Errors);
        var target = parsed.Data!;

        var exists = await identityService.AccountExistsAsync(target, ct);
        if (request.Purpose == OtpPurpose.Login && !exists)
        {
            return Result.Failure("No account found for that email or phone number. Please register first.");
        }
        if (request.Purpose == OtpPurpose.Register && exists)
        {
            return Result.Failure("An account already exists for that email or phone number. Please sign in.");
        }

        return await otpService.SendAsync(target, ct);
    }
}
