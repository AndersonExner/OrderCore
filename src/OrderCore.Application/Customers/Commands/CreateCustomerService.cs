using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Customers.Dtos;
using OrderCore.Domain.Entities;

namespace OrderCore.Application.Customers.Commands
{
    public class CreateCustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CreateCustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<CreateCustomerResponse> ExecuteAsync(
            CreateCustomerRequest request,
            CancellationToken cancellationToken = default)
        {
            var existingCustomer = await _customerRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (existingCustomer is not null)
                throw new InvalidOperationException("A customer with the same email already exists.");

            var newCustomer = new Customer(request.Name, request.Email);

            await _customerRepository.AddAsync(newCustomer, cancellationToken);

            return new CreateCustomerResponse
            {
                Id = newCustomer.Id,
                Name = newCustomer.Name,
                Email = newCustomer.Email
            };
        }
    }
}