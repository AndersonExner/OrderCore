using OrderCore.Application.Abstractions.Repositories;
using Microsoft.Extensions.Logging;
using OrderCore.Application.Customers.Dtos;
using OrderCore.Domain.Entities;
using OrderCore.Application.Common.Exceptions;
using OrderCore.Application.Common.Logging;

namespace OrderCore.Application.Customers.Commands
{
    public class CreateCustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CreateCustomerService> _logger;

        public CreateCustomerService(
            ICustomerRepository customerRepository,
            ILogger<CreateCustomerService> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        public async Task<CreateCustomerResponse> ExecuteAsync(
            CreateCustomerRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                ApplicationLogEvents.CustomerCreateStarted,
                "Creating customer. Email: {Email}",
                request.Email);

            var existingCustomer = await _customerRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (existingCustomer is not null)
            {
                _logger.LogWarning(
                    ApplicationLogEvents.CustomerCreateRejected,
                    "Customer creation rejected because email already exists. Email: {Email}",
                    request.Email);

                throw new BusinessRuleException("A customer with the same email already exists.");
            }

            var newCustomer = new Customer(request.Name, request.Email);

            await _customerRepository.AddAsync(newCustomer, cancellationToken);

            _logger.LogInformation(
                ApplicationLogEvents.CustomerCreated,
                "Customer created. CustomerId: {CustomerId}, Email: {Email}",
                newCustomer.Id,
                newCustomer.Email);

            return new CreateCustomerResponse
            {
                Id = newCustomer.Id,
                Name = newCustomer.Name,
                Email = newCustomer.Email
            };
        }
    }
}
