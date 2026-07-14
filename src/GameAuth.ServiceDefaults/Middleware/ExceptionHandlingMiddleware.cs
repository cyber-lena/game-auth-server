using System.Diagnostics;
using System.Text.Json;
using GameAuth.Shared.Constants;
using GameAuth.Shared.DTOs.Responses;
using GameAuth.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GameAuth.ServiceDefaults.Middleware;

/// <summary>
/// Catches unhandled exceptions and converts them into standardized
/// <see cref="ErrorResponse"/> / <see cref="ValidationErrorResponse"/> payloads.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        ErrorResponse response;
        int statusCode;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = validationException.StatusCode;
                response = new ValidationErrorResponse
                {
                    ErrorCode = validationException.ErrorCode,
                    Message = validationException.Message,
                    TraceId = traceId,
                    ValidationErrors = validationException.ValidationErrors
                };
                _logger.LogWarning(exception, "Validation failed. TraceId: {TraceId}", traceId);
                break;

            case AuthException authException:
                statusCode = authException.StatusCode;
                response = new ErrorResponse
                {
                    ErrorCode = authException.ErrorCode,
                    Message = authException.Message,
                    TraceId = traceId
                };
                _logger.LogWarning(exception, "Handled auth exception. TraceId: {TraceId}", traceId);
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                response = new ErrorResponse
                {
                    ErrorCode = ErrorCodes.InternalServerError,
                    Message = "An unexpected error occurred.",
                    TraceId = traceId
                };
                _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);
                break;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, response.GetType(), SerializerOptions));
    }
}
