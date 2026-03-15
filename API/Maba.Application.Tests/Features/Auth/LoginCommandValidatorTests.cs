using Maba.Application.Features.Auth.Commands;
using Maba.Application.Features.Auth.Validators;
using Xunit;

namespace Maba.Application.Tests.Features.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Should_HaveError_When_EmailIsEmpty()
    {
        var command = new LoginCommand { Email = "", Password = "SomePassword1!" };
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        var emailError = result.Errors.FirstOrDefault(e => e.PropertyName == nameof(LoginCommand.Email));
        Assert.NotNull(emailError);
        Assert.Equal("Email is required", emailError.ErrorMessage);
    }

    [Fact]
    public void Should_HaveError_When_EmailIsInvalidFormat()
    {
        var command = new LoginCommand { Email = "not-an-email", Password = "SomePassword1!" };
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        var emailError = result.Errors.FirstOrDefault(e => e.PropertyName == nameof(LoginCommand.Email));
        Assert.NotNull(emailError);
        Assert.Equal("Invalid email format", emailError.ErrorMessage);
    }

    [Fact]
    public void Should_NotHaveError_When_EmailIsValid()
    {
        var command = new LoginCommand { Email = "user@example.com", Password = "SomePassword1!" };
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_HaveError_When_PasswordIsEmpty()
    {
        var command = new LoginCommand { Email = "user@example.com", Password = "" };
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        var passwordError = result.Errors.FirstOrDefault(e => e.PropertyName == nameof(LoginCommand.Password));
        Assert.NotNull(passwordError);
        Assert.Equal("Password is required", passwordError.ErrorMessage);
    }

    [Fact]
    public void Should_NotHaveAnyErrors_When_CommandIsValid()
    {
        var command = new LoginCommand { Email = "user@example.com", Password = "ValidPass123!" };
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }
}
