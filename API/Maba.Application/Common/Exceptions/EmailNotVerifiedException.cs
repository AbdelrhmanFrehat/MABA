namespace Maba.Application.Common.Exceptions;

/// <summary>
/// Thrown when login is attempted with an unverified email. API should return 403 Forbidden.
/// </summary>
public class EmailNotVerifiedException : Exception
{
    public EmailNotVerifiedException() : base("Please verify your email before signing in.")
    {
    }

    public EmailNotVerifiedException(string message) : base(message)
    {
    }
}
