using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentValidation;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

public class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationExceptionMiddleware> _logger;

    public ValidationExceptionMiddleware(RequestDelegate next, ILogger<ValidationExceptionMiddleware> logger)
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
        catch (ValidationException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, "ValidationError", "Validation Failed", ex.Message, ex.Errors);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status404NotFound, "ResourceNotFound", "Resource not found", ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status401Unauthorized, "AuthenticationError", "Unauthorized", ex.Message);
        }
        catch (DomainException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, "DomainError", "Business rule violation", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status409Conflict, "ConflictError", "Operation conflict", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing request");
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "ServerError", "Internal server error", "An unexpected error occurred.");
        }
    }

    private static Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string type,
        string error,
        string detail,
        IEnumerable<FluentValidation.Results.ValidationFailure>? validationErrors = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new ApiResponse
        {
            Success = false,
            Message = error,
            Errors = validationErrors?.Select(e => (ValidationErrorDetail)e) ?? []
        };

        var payload = new
        {
            type,
            error,
            detail,
            success = response.Success,
            message = response.Message,
            errors = response.Errors
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, jsonOptions));
    }
}
