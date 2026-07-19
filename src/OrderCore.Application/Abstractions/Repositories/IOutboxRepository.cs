namespace OrderCore.Application.Abstractions.Repositories
{
    public interface IOutboxRepository
    {
        Task AddAsync(
            string type,
            string payload,
            CancellationToken cancellationToken = default);
    }
}
