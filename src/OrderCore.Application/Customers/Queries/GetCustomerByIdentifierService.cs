using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Common.Exceptions;
using OrderCore.Application.Customers.Dtos;

namespace OrderCore.Application.Customers.Queries
{
    public class GetCustomerByIdentifierService
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerByIdentifierService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<GetCustomerByIdResponse?> ExecuteAsync(
            string term,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(term))
                throw new ValidationException("Search term must be a valid customer id, email, or name.");

            var normalizedTerm = term.Trim();

            if (Guid.TryParse(normalizedTerm, out var customerId))
            {
                var customerById = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
                return customerById is null ? null : Map(customerById);
            }

            if (!IsEmail(normalizedTerm))
            {
                var customerByName = await _customerRepository.GetByNameAsync(
                    normalizedTerm,
                    cancellationToken);

                return customerByName is null ? null : Map(customerByName);
            }

            var customerByEmail = await _customerRepository.GetByEmailAsync(
                normalizedTerm.ToLowerInvariant(),
                cancellationToken);

            return customerByEmail is null ? null : Map(customerByEmail);
        }

        private static bool IsEmail(string term)
        {
            return term.Contains('@');
        }

        private static GetCustomerByIdResponse Map(Domain.Entities.Customer customer)
        {
            return new GetCustomerByIdResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email
            };
        }
    }
}
