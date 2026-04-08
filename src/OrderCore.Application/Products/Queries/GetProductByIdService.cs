using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Products.Dtos;

namespace OrderCore.Application.Products.Queries
{
    public class GetProductByIdService
    {
        private readonly IProductRepository _productRepository;

        public GetProductByIdService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<GetProductByIdResponse?> ExecuteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);

            if (product is null)
                return null;

            return new GetProductByIdResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                StockQuantity = product.StockQuantity
            };
        }
    }
}
