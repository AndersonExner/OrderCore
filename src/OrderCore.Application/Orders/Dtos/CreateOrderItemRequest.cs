using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCore.Application.Orders.Dtos
{
    public class CreateOrderItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
