using OrderCore.Application.Abstractions.Repositories;
using Microsoft.Extensions.Logging;
using OrderCore.Application.Common.Exceptions;
using OrderCore.Application.Common.Logging;
using OrderCore.Application.Orders.Dtos;
using OrderCore.Domain.Entities;

namespace OrderCore.Application.Orders.Commands
{
    public class CreateOrderService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<CreateOrderService> _logger;

        public CreateOrderService(
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            ILogger<CreateOrderService> logger)
        {
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<CreateOrderResponse> ExecuteAsync(
            CreateOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            var itemCount = request.Items?.Count ?? 0;

            _logger.LogInformation(
                ApplicationLogEvents.OrderCreateStarted,
                "Creating order. CustomerId: {CustomerId}, ItemCount: {ItemCount}",
                request.CustomerId,
                itemCount);

            if (request.Items is null || request.Items.Count == 0)
            {
                _logger.LogWarning(
                    ApplicationLogEvents.OrderCreateRejected,
                    "Order creation rejected because item list is empty. CustomerId: {CustomerId}",
                    request.CustomerId);

                throw new ValidationException("Order must have at least one item.");
            }

            if (request.Items.Any(x => x.Quantity <= 0))
            {
                _logger.LogWarning(
                    ApplicationLogEvents.OrderCreateRejected,
                    "Order creation rejected because at least one item has invalid quantity. CustomerId: {CustomerId}",
                    request.CustomerId);

                throw new ValidationException("Order item quantity must be greater than zero.");
            }

            var productIds = request.Items
                .Select(x => x.ProductId)
                .ToList();

            if (productIds.Distinct().Count() != productIds.Count)
            {
                var duplicatedProductIds = string.Join(",", productIds);

                _logger.LogWarning(
                    ApplicationLogEvents.OrderCreateRejected,
                    "Order creation rejected because duplicate products were informed. CustomerId: {CustomerId}, ProductIds: {ProductIds}",
                    request.CustomerId,
                    duplicatedProductIds);

                throw new ValidationException("Order cannot contain duplicate products.");
            }

            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

            if (customer is null)
            {
                _logger.LogWarning(
                    ApplicationLogEvents.OrderCreateRejected,
                    "Order creation rejected because customer was not found. CustomerId: {CustomerId}",
                    request.CustomerId);

                throw new NotFoundException("Customer not found.");
            }

            var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);
            var productsById = products.ToDictionary(x => x.Id);
            var order = new Order(request.CustomerId);

            foreach (var itemRequest in request.Items)
            {
                if (!productsById.TryGetValue(itemRequest.ProductId, out var product))
                {
                    _logger.LogWarning(
                        ApplicationLogEvents.OrderCreateRejected,
                        "Order creation rejected because product was not found. CustomerId: {CustomerId}, ProductId: {ProductId}",
                        request.CustomerId,
                        itemRequest.ProductId);

                    throw new NotFoundException($"Product {itemRequest.ProductId} not found.");
                }

                try
                {
                    product.DecreaseStock(itemRequest.Quantity);
                    order.AddItem(product.Id, product.Name, product.Price, itemRequest.Quantity);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(
                        ApplicationLogEvents.OrderCreateRejected,
                        ex,
                        "Order creation rejected by business rule. CustomerId: {CustomerId}, ProductId: {ProductId}, RequestedQuantity: {RequestedQuantity}, Reason: {Reason}",
                        request.CustomerId,
                        itemRequest.ProductId,
                        itemRequest.Quantity,
                        ex.Message);

                    throw new BusinessRuleException(ex.Message);
                }
            }

            await _orderRepository.AddAsync(order, cancellationToken);

            _logger.LogInformation(
                ApplicationLogEvents.OrderCreated,
                "Order created. OrderId: {OrderId}, CustomerId: {CustomerId}, ItemCount: {ItemCount}, TotalAmount: {TotalAmount}",
                order.Id,
                order.CustomerId,
                order.Items.Count,
                order.TotalAmount);

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
