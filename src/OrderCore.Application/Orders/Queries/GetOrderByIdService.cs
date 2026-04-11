using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Orders.Dtos;

namespace OrderCore.Application.Orders.Queries
{
    public class GetOrderByIdService
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderByIdService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<GetOrderByIdResponse?> ExecuteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

            if (order is null)
                return null;

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