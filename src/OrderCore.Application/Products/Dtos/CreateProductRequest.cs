using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCore.Application.Products.Dtos
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
