using OrderCore.Api.Models;
using OrderCore.Application.Commom.Exceptions;
using OrderCore.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace OrderCore.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
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