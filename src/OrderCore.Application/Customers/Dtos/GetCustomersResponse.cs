namespace OrderCore.Application.Customers.Dtos
{
    public sealed class GetCustomersResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
    }
}