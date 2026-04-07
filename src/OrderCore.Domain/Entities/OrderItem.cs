using OrderCore.Domain.Common;

namespace OrderCore.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    public decimal Total => UnitPrice * Quantity;

    private OrderItem()
    {
        ProductName = string.Empty;
    }

    public OrderItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty.");

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty.");

        if (unitPrice <= 0)
            throw new ArgumentException("Unit price must be greater than zero.");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        ProductId = productId;
        ProductName = productName.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
}