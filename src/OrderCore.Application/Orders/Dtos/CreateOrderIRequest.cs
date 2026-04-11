using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCore.Application.Orders.Dtos
{
    public class CreateOrderRequest
    {
        public Guid CustomerId { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; } = new ();
    }
}
