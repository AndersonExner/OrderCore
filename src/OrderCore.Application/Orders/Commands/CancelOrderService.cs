using Microsoft.Extensions.Logging;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Common.Exceptions;
using OrderCore.Application.Common.Logging;
using OrderCore.Application.Orders.Dtos;
using OrderCore.Domain.Enums;

namespace OrderCore.Application.Orders.Commands
{
    public class CancelOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<CancelOrderService> _logger;

        public CancelOrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ILogger<CancelOrderService> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<GetOrderByIdResponse> ExecuteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                ApplicationLogEvents.OrderCancelStarted,
                "Cancelling order. OrderId: {OrderId}",
                id);

            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

            if (order is null)
            {
                _logger.LogWarning(
                    ApplicationLogEvents.OrderCancelRejected,
                    "Order cancellation rejected because order was not found. OrderId: {OrderId}",
                    id);

                throw new NotFoundException("Order not found.");
            }

            var wasPending = order.Status == OrderStatus.Pending;
            var restoredStockQuantity = 0;

            try
            {
                order.Cancel();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    ApplicationLogEvents.OrderCancelRejected,
                    ex,
                    "Order cancellation rejected by business rule. OrderId: {OrderId}, CurrentStatus: {CurrentStatus}, Reason: {Reason}",
                    id,
                    order.Status,
                    ex.Message);

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
                    {
                        _logger.LogWarning(
                            ApplicationLogEvents.OrderCancelRejected,
                            "Order cancellation rejected because product was not found while restoring stock. OrderId: {OrderId}, ProductId: {ProductId}",
                            order.Id,
                            item.ProductId);

                        throw new NotFoundException($"Product {item.ProductId} not found.");
                    }

                    product.IncreaseStock(item.Quantity);
                    restoredStockQuantity += item.Quantity;
                }
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);

            _logger.LogInformation(
                ApplicationLogEvents.OrderCancelled,
                "Order cancelled. OrderId: {OrderId}, CustomerId: {CustomerId}, RestoredStockQuantity: {RestoredStockQuantity}, ItemCount: {ItemCount}",
                order.Id,
                order.CustomerId,
                restoredStockQuantity,
                order.Items.Count);

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
