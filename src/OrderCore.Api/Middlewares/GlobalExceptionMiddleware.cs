using OrderCore.Api.Models;
using OrderCore.Api.Logging;
using OrderCore.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace OrderCore.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
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
                LogException(context, ex);
                await HandleExceptionAsync(context, ex);
            }
        }

        private void LogException(HttpContext context, Exception exception)
        {
            var isExpectedException = exception is ValidationException
                or BusinessRuleException
                or NotFoundException
                or ArgumentException;

            const string message =
                "HTTP request failed. Method: {Method}, Path: {Path}, TraceIdentifier: {TraceIdentifier}, ExceptionType: {ExceptionType}";

            if (isExpectedException)
            {
                _logger.LogWarning(
                    ApiLogEvents.RequestExceptionHandled,
                    exception,
                    message,
                    context.Request.Method,
                    context.Request.Path,
                    context.TraceIdentifier,
                    exception.GetType().Name);

                return;
            }

            _logger.LogError(
                ApiLogEvents.RequestExceptionHandled,
                exception,
                message,
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier,
                exception.GetType().Name);
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = new ErrorResponse();
            var statusCode = HttpStatusCode.InternalServerError;

            switch (exception)
            {
                case ValidationException:
                    statusCode = HttpStatusCode.BadRequest;
                    response.Title = "Validation error";
                    response.Detail = exception.Message;
                    break;

                case BusinessRuleException:
                    statusCode = HttpStatusCode.Conflict;
                    response.Title = "Business rule violation";
                    response.Detail = exception.Message;
                    break;

                case NotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    response.Title = "Resource not found";
                    response.Detail = exception.Message;
                    break;

                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest;
                    response.Title = "Invalid request";
                    response.Detail = exception.Message;
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    response.Title = "Unexpected error";
                    response.Detail = "An unexpected error occurred.";
                    break;
            }

            response.Status = (int)statusCode;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}
