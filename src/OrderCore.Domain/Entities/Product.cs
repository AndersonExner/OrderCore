using OrderCore.Domain.Common;

namespace OrderCore.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }

    private Product()
    {
        Name = string.Empty;
    }

    public Product(string name, decimal price, int stockQuantity)
    {
        SetName(name);
        SetPrice(price);
        SetStock(stockQuantity);
    }

    public void Update(string name, decimal price)
    {
        SetName(name);
        SetPrice(price);
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        StockQuantity += quantity;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        if (StockQuantity < quantity)
            throw new InvalidOperationException("Insufficient stock.");

        StockQuantity -= quantity;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.");

        Name = name.Trim();
    }

    private void SetPrice(decimal price)
    {
        if (price <= 0)
            throw new ArgumentException("Product price must be greater than zero.");

        Price = price;
    }

    private void SetStock(int stockQuantity)
    {
        if (stockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative.");

        StockQuantity = stockQuantity;
    }
}