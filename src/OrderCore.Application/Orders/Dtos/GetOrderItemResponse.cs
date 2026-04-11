using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCore.Application.Orders.Dtos
{
    public class GetOrderItemResponse
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }
}
