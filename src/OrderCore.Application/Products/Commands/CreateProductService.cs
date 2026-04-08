using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Products.Dtos;
using OrderCore.Domain.Entities;

namespace OrderCore.Application.Products.Commands
{
    public class CreateProductService
    {
        private readonly IProductRepository _productRepository;

        public CreateProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<CreateProductResponse> ExecuteAsync(
            CreateProductRequest request,
            CancellationToken cancellationToken = default)
        {
            var product = new Product(request.Name, request.Price, request.StockQuantity);

            await _productRepository.AddAsync(product, cancellationToken);

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
