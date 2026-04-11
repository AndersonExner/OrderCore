using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Orders.Dtos;

namespace OrderCore.Application.Orders.Queries
{
    public class GetOrdersService
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrdersService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IReadOnlyList<GetOrdersResponse>> ExecuteAsync(
            CancellationToken cancellationToken = default)
        {
            var orders = await _orderRepository.GetAllAsync(cancellationToken);

            return orders
                .Select(x => new GetOrdersResponse
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    Status = x.Status.ToString(),
                    CreatedAtUtc = x.CreatedAtUtc,
                    TotalAmount = x.TotalAmount
                })
                .ToList();
        }
    }
}