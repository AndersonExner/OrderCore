using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Products.Dtos;

namespace OrderCore.Application.Products.Queries
{
    public class GetProductsService
    {
        private readonly IProductRepository _productRepository;

        public GetProductsService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IReadOnlyList<GetProductsResponse>> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var products = await _productRepository.GetAllAsync(cancellationToken);
            return products.Select(p => new GetProductsResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                StockQuantity = p.StockQuantity
            }).ToList();
        }
    }
}
