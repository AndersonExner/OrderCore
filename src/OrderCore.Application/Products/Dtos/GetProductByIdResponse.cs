using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCore.Application.Products.Dtos
{
    public class GetProductByIdResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
