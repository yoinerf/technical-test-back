using InvestmentFunds.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace InvestmentFunds.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        _logger.LogError(exception, "Ocurrió una excepción no controlada");

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            Message = exception.Message,
            TraceId = context.TraceIdentifier
        };

        switch (exception)
        {
            case InsufficientBalanceException:
            case FundNotFoundException:
            case SubscriptionNotFoundException:
            case CustomerNotFoundException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Error = "BusinessError";
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Error = "Unauthorized";
                errorResponse.Message = "No autorizado para realizar esta operación";
                break;

            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Error = "ValidationError";
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Error = "InternalServerError";
                errorResponse.Message = "Ocurrió un error interno en el servidor";
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(result);
    }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
}