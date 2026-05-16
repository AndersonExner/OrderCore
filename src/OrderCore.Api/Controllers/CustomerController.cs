using Microsoft.AspNetCore.Mvc;
using OrderCore.Application.Customers.Commands;
using OrderCore.Application.Customers.Dtos;
using OrderCore.Application.Customers.Queries;

namespace OrderCore.Api.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly CreateCustomerService _createCustomerService;
        private readonly GetCustomerByIdService _getCustomerByIdService;
        private readonly GetCustomersService _getCustomersService;

        public CustomersController(
            CreateCustomerService createCustomerService,
            GetCustomerByIdService getCustomerByIdService,
            GetCustomersService getCustomersService)
        {
            _createCustomerService = createCustomerService;
            _getCustomerByIdService = getCustomerByIdService;
            _getCustomersService = getCustomersService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateCustomerResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] CreateCustomerRequest request,
            CancellationToken cancellationToken)
        {
            var response = await _createCustomerService.ExecuteAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = response.Id },
                response);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(GetCustomerByIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var response = await _getCustomerByIdService.ExecuteAsync(id, cancellationToken);

            if (response is null)
                return NotFound();

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<GetCustomersResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await _getCustomersService.ExecuteAsync(cancellationToken);
            return Ok(response);
        }
    }
}


