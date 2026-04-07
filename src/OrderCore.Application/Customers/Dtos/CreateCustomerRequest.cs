using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCore.Application.Customers.Dtos
{
    public class CreateCustomerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
