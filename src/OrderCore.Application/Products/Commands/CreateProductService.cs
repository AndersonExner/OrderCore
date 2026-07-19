using OrderCore.Application.Abstractions.Repositories;
using Microsoft.Extensions.Logging;
using OrderCore.Application.Common.Logging;
using OrderCore.Application.Products.Dtos;
using OrderCore.Domain.Entities;

namespace OrderCore.Application.Products.Commands
{
    public class CreateProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<CreateProductService> _logger;

        public CreateProductService(
            IProductRepository productRepository,
            ILogger<CreateProductService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<CreateProductResponse> ExecuteAsync(
            CreateProductRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                ApplicationLogEvents.ProductCreateStarted,
                "Creating product. Name: {ProductName}, Price: {Price}, StockQuantity: {StockQuantity}",
                request.Name,
                request.Price,
                request.StockQuantity);

            var product = new Product(request.Name, request.Price, request.StockQuantity);

            await _productRepository.AddAsync(product, cancellationToken);

            _logger.LogInformation(
                ApplicationLogEvents.ProductCreated,
                "Product created. ProductId: {ProductId}, Name: {ProductName}, StockQuantity: {StockQuantity}",
                product.Id,
                product.Name,
                product.StockQuantity);

            return new CreateProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                StockQuantity = product.StockQuantity
            };
        }
    }
}
