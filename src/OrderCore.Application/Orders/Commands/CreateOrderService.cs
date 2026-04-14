using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Commom.Exceptions;
using OrderCore.Application.Orders.Dtos;
using OrderCore.Domain.Entities;

namespace OrderCore.Application.Orders.Commands
{
    public class CreateOrderService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;

        public CreateOrderService(
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IOrderRepository orderRepository)
        {
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
        }

        public async Task<CreateOrderResponse> ExecuteAsync(
            CreateOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.Items is null || request.Items.Count == 0)
                throw new ValidationException("Order must have at least one item.");

            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

            if (customer is null)
                throw new NotFoundException("Customer not found.");

            var order = new Order(request.CustomerId);

            foreach (var itemRequest in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(itemRequest.ProductId, cancellationToken);

                if (product is null)
                    throw new NotFoundException($"Product {itemRequest.ProductId} not found.");

                order.AddItem(product.Id, product.Name, product.Price, itemRequest.Quantity);
            }

            await _orderRepository.AddAsync(order, cancellationToken);

            return new CreateOrderResponse
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Status = order.Status.ToString(),
                CreatedAtUtc = order.CreatedAtUtc,
                TotalAmount = order.TotalAmount,
                Items = order.Items.Select(x => new CreateOrderItemResponse
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