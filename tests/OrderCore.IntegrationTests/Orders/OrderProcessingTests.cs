using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using OrderCore.Application.Common.Outbox;
using OrderCore.Infrastructure.Persistence;

namespace OrderCore.IntegrationTests.Orders;

public class OrderProcessingTests : IClassFixture<OrderCoreApiFactory>
{
    private readonly HttpClient _client;
    private readonly OrderCoreApiFactory _factory;

    public OrderProcessingTests(OrderCoreApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_Create_Order_And_Decrease_Product_Stock()
    {
        // Arrange
        var customer = await CreateCustomerAsync();
        var product = await CreateProductAsync(stockQuantity: 10);

        // Act
        var order = await CreateOrderAsync(customer.Id, product.Id, quantity: 2);
        var updatedProduct = await GetProductAsync(product.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Created, order.StatusCode);
        Assert.NotNull(order.Body);
        Assert.Equal("Pending", order.Body.Status);
        Assert.Equal(9000m, order.Body.TotalAmount);
        Assert.Equal(8, updatedProduct.StockQuantity);
    }

    [Fact]
    public async Task Should_Return_Conflict_When_Product_Has_Insufficient_Stock()
    {
        // Arrange
        var customer = await CreateCustomerAsync();
        var product = await CreateProductAsync(stockQuantity: 1);

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            customerId = customer.Id,
            items = new[]
            {
                new
                {
                    productId = product.Id,
                    quantity = 2
                }
            }
        });

        var updatedProduct = await GetProductAsync(product.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(1, updatedProduct.StockQuantity);
    }

    [Fact]
    public async Task Should_Mark_Order_As_Paid()
    {
        // Arrange
        var customer = await CreateCustomerAsync();
        var product = await CreateProductAsync(stockQuantity: 10);
        var order = await CreateOrderAsync(customer.Id, product.Id, quantity: 2);

        Assert.NotNull(order.Body);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/orders/{order.Body.Id}/pay", new { });
        var paidOrder = await response.Content.ReadFromJsonAsync<OrderResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paidOrder);
        Assert.Equal("Paid", paidOrder.Status);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var outboxMessage = dbContext.OutboxMessages
            .Where(x => x.Type == OutboxMessageTypes.OrderPaid)
            .AsEnumerable()
            .Single(x => x.Payload.Contains(order.Body.Id.ToString()));

        Assert.Equal(OutboxMessageStatus.Pending, outboxMessage.Status);
        Assert.Equal(0, outboxMessage.RetryCount);
        Assert.Null(outboxMessage.ProcessedAtUtc);
    }

    [Fact]
    public async Task Should_Process_Pending_Outbox_Message()
    {
        // Arrange
        var customer = await CreateCustomerAsync();
        var product = await CreateProductAsync(stockQuantity: 10);
        var order = await CreateOrderAsync(customer.Id, product.Id, quantity: 2);

        Assert.NotNull(order.Body);

        var payResponse = await _client.PostAsJsonAsync($"/api/orders/{order.Body.Id}/pay", new { });
        Assert.Equal(HttpStatusCode.OK, payResponse.StatusCode);

        // Act
        using var scope = _factory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<OutboxMessageProcessorService>();
        var processedCount = await processor.ExecuteAsync(batchSize: 20, maxRetryCount: 5);

        // Assert
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var outboxMessage = dbContext.OutboxMessages
            .Where(x => x.Type == OutboxMessageTypes.OrderPaid)
            .AsEnumerable()
            .Single(x => x.Payload.Contains(order.Body.Id.ToString()));

        Assert.True(processedCount >= 1);
        Assert.Equal(OutboxMessageStatus.Processed, outboxMessage.Status);
        Assert.NotNull(outboxMessage.ProcessedAtUtc);
    }

    [Fact]
    public async Task Should_Cancel_Pending_Order_And_Restore_Product_Stock()
    {
        // Arrange
        var customer = await CreateCustomerAsync();
        var product = await CreateProductAsync(stockQuantity: 10);
        var order = await CreateOrderAsync(customer.Id, product.Id, quantity: 2);

        Assert.NotNull(order.Body);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/orders/{order.Body.Id}/cancel", new { });
        var cancelledOrder = await response.Content.ReadFromJsonAsync<OrderResponse>();
        var updatedProduct = await GetProductAsync(product.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(cancelledOrder);
        Assert.Equal("Cancelled", cancelledOrder.Status);
        Assert.Equal(10, updatedProduct.StockQuantity);
    }

    [Fact]
    public async Task Should_Return_Conflict_When_Cancelling_Paid_Order()
    {
        // Arrange
        var customer = await CreateCustomerAsync();
        var product = await CreateProductAsync(stockQuantity: 10);
        var order = await CreateOrderAsync(customer.Id, product.Id, quantity: 2);

        Assert.NotNull(order.Body);

        var payResponse = await _client.PostAsJsonAsync($"/api/orders/{order.Body.Id}/pay", new { });
        Assert.Equal(HttpStatusCode.OK, payResponse.StatusCode);

        // Act
        var cancelResponse = await _client.PostAsJsonAsync($"/api/orders/{order.Body.Id}/cancel", new { });
        var updatedProduct = await GetProductAsync(product.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, cancelResponse.StatusCode);
        Assert.Equal(8, updatedProduct.StockQuantity);
    }

    private async Task<CustomerResponse> CreateCustomerAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/customers", new
        {
            name = $"Customer {Guid.NewGuid():N}",
            email = $"customer-{Guid.NewGuid():N}@email.com"
        });

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<CustomerResponse>())!;
    }

    private async Task<ProductResponse> CreateProductAsync(int stockQuantity)
    {
        var response = await _client.PostAsJsonAsync("/api/products", new
        {
            name = $"Notebook {Guid.NewGuid():N}",
            price = 4500m,
            stockQuantity
        });

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
    }

    private async Task<ProductResponse> GetProductAsync(Guid id)
    {
        var product = await _client.GetFromJsonAsync<ProductResponse>($"/api/products/{id}");

        return product!;
    }

    private async Task<ApiResponse<OrderResponse>> CreateOrderAsync(Guid customerId, Guid productId, int quantity)
    {
        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            customerId,
            items = new[]
            {
                new
                {
                    productId,
                    quantity
                }
            }
        });

        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();

        return new ApiResponse<OrderResponse>(response.StatusCode, body);
    }

    private sealed record ApiResponse<T>(HttpStatusCode StatusCode, T? Body);

    private sealed record CustomerResponse(Guid Id);

    private sealed record ProductResponse(Guid Id, int StockQuantity);

    private sealed record OrderResponse(
        Guid Id,
        string Status,
        decimal TotalAmount,
        IReadOnlyList<OrderItemResponse> Items);

    private sealed record OrderItemResponse(
        Guid ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity,
        decimal Total);
}
