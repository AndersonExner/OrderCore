using Microsoft.AspNetCore.Mvc;
using OrderCore.Application.Orders.Commands;
using OrderCore.Application.Orders.Dtos;
using OrderCore.Application.Orders.Queries;

namespace OrderCore.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly CreateOrderService _createOrderService;
        private readonly PayOrderService _payOrderService;
        private readonly CancelOrderService _cancelOrderService;
        private readonly GetOrderByIdService _getOrderByIdService;
        private readonly GetOrdersService _getOrdersService;

        public OrdersController(
            CreateOrderService createOrderService,
            PayOrderService payOrderService,
            CancelOrderService cancelOrderService,
            GetOrderByIdService getOrderByIdService,
            GetOrdersService getOrdersService)
        {
            _createOrderService = createOrderService;
            _payOrderService = payOrderService;
            _cancelOrderService = cancelOrderService;
            _getOrderByIdService = getOrderByIdService;
            _getOrdersService = getOrdersService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create(
            [FromBody] CreateOrderRequest request,
            CancellationToken cancellationToken)
        {
            var response = await _createOrderService.ExecuteAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPost("{id:guid}/pay")]
        [ProducesResponseType(typeof(GetOrderByIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Pay(Guid id, CancellationToken cancellationToken)
        {
            var response = await _payOrderService.ExecuteAsync(id, cancellationToken);

            return Ok(response);
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(GetOrderByIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        {
            var response = await _cancelOrderService.ExecuteAsync(id, cancellationToken);

            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(GetOrderByIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var response = await _getOrderByIdService.ExecuteAsync(id, cancellationToken);

            if (response is null)
                return NotFound();

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<GetOrdersResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await _getOrdersService.ExecuteAsync(cancellationToken);

            return Ok(response);
        }
    }
}
