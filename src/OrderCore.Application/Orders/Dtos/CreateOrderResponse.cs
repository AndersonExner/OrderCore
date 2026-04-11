using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCore.Application.Orders.Dtos
{
    public class CreateOrderResponse
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public decimal TotalAmount { get; set; }
        public List<CreateOrderItemResponse> Items { get; set; } = new();
    }
}
