using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Common.Exceptions;
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

            if (request.Items.Any(x => x.Quantity <= 0))
                throw new ValidationException("Order item quantity must be greater than zero.");

            var productIds = request.Items
                .Select(x => x.ProductId)
                .ToList();

            if (productIds.Distinct().Count() != productIds.Count)
                throw new ValidationException("Order cannot contain duplicate products.");

            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

            if (customer is null)
                throw new NotFoundException("Customer not found.");

            var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);
            var productsById = products.ToDictionary(x => x.Id);
            var order = new Order(request.CustomerId);

            foreach (var itemRequest in request.Items)
            {
                if (!productsById.TryGetValue(itemRequest.ProductId, out var product))
                    throw new NotFoundException($"Product {itemRequest.ProductId} not found.");

                try
                {
                    product.DecreaseStock(itemRequest.Quantity);
                    order.AddItem(product.Id, product.Name, product.Price, itemRequest.Quantity);
                }
                catch (InvalidOperationException ex)
                {
                    throw new BusinessRuleException(ex.Message);
                }
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
