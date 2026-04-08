using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Customers.Dtos;

namespace OrderCore.Application.Customers.Queries
{
    public class GetCustomerByIdService
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerByIdService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<GetCustomerByIdResponse?> ExecuteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);

            if (customer is null)
                return null;

            return new GetCustomerByIdResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email
            };
        }
    }
}