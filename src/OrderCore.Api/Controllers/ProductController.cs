using Microsoft.AspNetCore.Mvc;
using OrderCore.Application.Products.Commands;
using OrderCore.Application.Products.Dtos;
using OrderCore.Application.Products.Queries;

namespace OrderCore.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly CreateProductService _createProductService;
        private readonly GetProductByIdService _getProductByIdService;
        private readonly GetProductsService _getProductsService;

        public ProductsController(
            CreateProductService createProductService,
            GetProductByIdService getProductByIdService,
            GetProductsService getProductsService)
        {
            _createProductService = createProductService;
            _getProductByIdService = getProductByIdService;
            _getProductsService = getProductsService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateProductResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] CreateProductRequest request,
            CancellationToken cancellationToken)
        {
            var response = await _createProductService.ExecuteAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = response.Id },
                response);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(GetProductByIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var response = await _getProductByIdService.ExecuteAsync(id, cancellationToken);

            if (response is null)
                return NotFound();

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<GetProductsResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await _getProductsService.ExecuteAsync(cancellationToken);

            return Ok(response);
        }
    }
}