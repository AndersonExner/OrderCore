using Microsoft.Extensions.Logging;

namespace OrderCore.Api.Logging
{
    public static class ApiLogEvents
    {
        public static readonly EventId RequestExceptionHandled = new(9001, nameof(RequestExceptionHandled));
        public static readonly EventId OutboxBackgroundProcessingFailed = new(9101, nameof(OutboxBackgroundProcessingFailed));
    }
}
