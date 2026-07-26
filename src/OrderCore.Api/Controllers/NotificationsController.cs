using Microsoft.AspNetCore.Mvc;
using OrderCore.Application.Notifications.Commands;
using OrderCore.Application.Notifications.Dtos;
using OrderCore.Application.Notifications.Queries;

namespace OrderCore.Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly GetNotificationsService _getNotificationsService;
        private readonly MarkNotificationAsReadService _markNotificationAsReadService;

        public NotificationsController(
            GetNotificationsService getNotificationsService,
            MarkNotificationAsReadService markNotificationAsReadService)
        {
            _getNotificationsService = getNotificationsService;
            _markNotificationAsReadService = markNotificationAsReadService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<NotificationResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecent(
            [FromQuery] bool unreadOnly = false,
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            var response = await _getNotificationsService.ExecuteAsync(
                unreadOnly,
                limit,
                cancellationToken);

            return Ok(response);
        }

        [HttpPost("{id:guid}/read")]
        [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsRead(
            Guid id,
            CancellationToken cancellationToken)
        {
            var response = await _markNotificationAsReadService.ExecuteAsync(id, cancellationToken);

            return Ok(response);
        }
    }
}
