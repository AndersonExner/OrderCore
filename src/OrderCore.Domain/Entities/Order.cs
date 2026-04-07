using OrderCore.Domain.Common;
using OrderCore.Domain.Enums;

namespace OrderCore.Domain.Entities;

public class Order : BaseEntity
{
    private readonly List<OrderItem> _items = new();

    public Guid CustomerId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public OrderStatus Status { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(x => x.Total);

    private Order()
    {
    }

    public Order(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty.");

        CustomerId = customerId;
        CreatedAtUtc = DateTime.UtcNow;
        Status = OrderStatus.Pending;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Items can only be added to pending orders.");

        var existingItem = _items.FirstOrDefault(x => x.ProductId == productId);

        if (existingItem is not null)
            throw new InvalidOperationException("Product already exists in the order.");

        var item = new OrderItem(productId, productName, unitPrice, quantity);
        _items.Add(item);
    }

    public void MarkAsPaid()
    {
        if (!_items.Any())
            throw new InvalidOperationException("Order cannot be paid without items.");

        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be marked as paid.");

        Status = OrderStatus.Paid;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Paid)
            throw new InvalidOperationException("Paid orders cannot be cancelled.");

        if (Status == OrderStatus.Cancelled)
            return;

        Status = OrderStatus.Cancelled;
    }
}