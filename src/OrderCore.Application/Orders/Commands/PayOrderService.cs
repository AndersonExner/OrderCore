using System.Text.Json;
using Microsoft.Extensions.Logging;
using OrderCore.Application.Abstractions.Persistence;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Common.Exceptions;
using OrderCore.Application.Common.Logging;
using OrderCore.Application.Common.Outbox;
using OrderCore.Application.Orders.Dtos;

namespace OrderCore.Application.Orders.Commands
{
    public class PayOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PayOrderService> _logger;

        public PayOrderService(
            IOrderRepository orderRepository,
            IOutboxRepository outboxRepository,
            IUnitOfWork unitOfWork,
            ILogger<PayOrderService> logger)
        {
            _orderRepository = orderRepository;
            _outboxRepository = outboxRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<GetOrderByIdResponse> ExecuteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            Domain.Entities.Order? order = null;

            _logger.LogInformation(
                ApplicationLogEvents.OrderPayStarted,
                "Paying order. OrderId: {OrderId}",
                id);

            await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
            {
                order = await _orderRepository.GetByIdAsync(id, transactionCancellationToken);

                if (order is null)
                {
                    _logger.LogWarning(
                        ApplicationLogEvents.OrderPayRejected,
                        "Order payment rejected because order was not found. OrderId: {OrderId}",
                        id);

                    throw new NotFoundException("Order not found.");
                }

                try
                {
                    order.MarkAsPaid();
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(
                        ApplicationLogEvents.OrderPayRejected,
                        ex,
                        "Order payment rejected by business rule. OrderId: {OrderId}, Reason: {Reason}",
                        id,
                        ex.Message);

                    throw new BusinessRuleException(ex.Message);
                }

                await _orderRepository.UpdateAsync(order, transactionCancellationToken);

                var outboxPayload = JsonSerializer.Serialize(new OrderPaidEvent(
                    order.Id,
                    order.CustomerId,
                    order.TotalAmount,
                    DateTime.UtcNow));

                await _outboxRepository.AddAsync(
                    OutboxMessageTypes.OrderPaid,
                    outboxPayload,
                    transactionCancellationToken);
            }, cancellationToken);

            if (order is null)
            {
                _logger.LogWarning(
                    ApplicationLogEvents.OrderPayRejected,
                    "Order payment rejected because order was not found after transaction. OrderId: {OrderId}",
                    id);

                throw new NotFoundException("Order not found.");
            }

            _logger.LogInformation(
                ApplicationLogEvents.OrderPaid,
                "Order paid. OrderId: {OrderId}, CustomerId: {CustomerId}, TotalAmount: {TotalAmount}, OutboxMessageType: {OutboxMessageType}",
                order.Id,
                order.CustomerId,
                order.TotalAmount,
                OutboxMessageTypes.OrderPaid);

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

        private sealed record OrderPaidEvent(
            Guid OrderId,
            Guid CustomerId,
            decimal TotalAmount,
            DateTime PaidAtUtc);
    }
}
