using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Customers.Dtos;

namespace OrderCore.Application.Customers.Queries
{
    public sealed class GetCustomersService
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomersService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<IReadOnlyList<GetCustomersResponse>> ExecuteAsync(CancellationToken cancellationToken)
        {
            var customers = await _customerRepository.GetAllAsync(cancellationToken);

            return customers
                .Select(customer => new GetCustomersResponse
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Email = customer.Email
                })
                .ToList();
        }
    }
}