using System.Net;
using System.Text.Json;
using OrderCore.Api.Models;

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
                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest;
                    response.Title = "Invalid request";
                    response.Detail = exception.Message;
                    break;

                case InvalidOperationException:
                    statusCode = HttpStatusCode.BadRequest;
                    response.Title = "Business rule violation";
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