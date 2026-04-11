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
        private readonly GetOrderByIdService _getOrderByIdService;
        private readonly GetOrdersService _getOrdersService;

        public OrdersController(
            CreateOrderService createOrderService,
            GetOrderByIdService getOrderByIdService,
            GetOrdersService getOrdersService)
        {
            _createOrderService = createOrderService;
            _getOrderByIdService = getOrderByIdService;
            _getOrdersService = getOrdersService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] CreateOrderRequest request,
            CancellationToken cancellationToken)
        {
            var response = await _createOrderService.ExecuteAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
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