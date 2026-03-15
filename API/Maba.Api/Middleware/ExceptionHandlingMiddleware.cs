using System.Net;
using System.Text.Json;
using ValidationException = Maba.Application.Common.Exceptions.ValidationException;
using EmailNotVerifiedException = Maba.Application.Common.Exceptions.EmailNotVerifiedException;

namespace Maba.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var result = string.Empty;

        switch (exception)
        {
            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                result = JsonSerializer.Serialize(new { error = "Resource not found", message = exception.Message });
                break;
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                result = JsonSerializer.Serialize(new { error = "Unauthorized", message = exception.Message });
                break;
            case EmailNotVerifiedException:
                code = HttpStatusCode.Forbidden;
                result = JsonSerializer.Serialize(new { error = "Email not verified", message = exception.Message });
                break;
            case ValidationException validationException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new { error = "Validation failed", errors = validationException.Errors });
                break;
            case FluentValidation.ValidationException fluentValidationException:
                code = HttpStatusCode.BadRequest;
                var errors = fluentValidationException.Errors
                    .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                    .ToDictionary(g => g.Key, g => g.ToArray());
                result = JsonSerializer.Serialize(new { error = "Validation failed", errors = errors });
                break;
            case ArgumentException:
            case InvalidOperationException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new { error = "Bad request", message = exception.Message });
                break;
            default:
                _logger.LogError(exception, "An unhandled exception occurred");
                result = JsonSerializer.Serialize(new { error = "An error occurred while processing your request" });
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}

