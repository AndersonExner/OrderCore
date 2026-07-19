using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Common.Exceptions;
using OrderCore.Application.Orders.Dtos;
using OrderCore.Domain.Enums;

namespace OrderCore.Application.Orders.Commands
{
    public class CancelOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public CancelOrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task<GetOrderByIdResponse> ExecuteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

            if (order is null)
                throw new NotFoundException("Order not found.");

            var wasPending = order.Status == OrderStatus.Pending;

            try
            {
                order.Cancel();
            }
            catch (InvalidOperationException ex)
            {
                throw new BusinessRuleException(ex.Message);
            }

            if (wasPending)
            {
                var productIds = order.Items
                    .Select(x => x.ProductId)
                    .ToList();

                var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);
                var productsById = products.ToDictionary(x => x.Id);

                foreach (var item in order.Items)
                {
                    if (!productsById.TryGetValue(item.ProductId, out var product))
                        throw new NotFoundException($"Product {item.ProductId} not found.");

                    product.IncreaseStock(item.Quantity);
                }
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);

            return new GetOrderByIdResponse
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Status = order.Status.ToString(),
                CreatedAtUtc = order.CreatedAtUtc,
                TotalAmount = order.TotalAmount,
                Items = order.Items.Select(x => new GetOrderItemResponse
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    UnitPrice = x.UnitPrice,
                    Quantity = x.Quantity,
                    Total = x.Total
                }).ToList()
            };
        }
    }
}
