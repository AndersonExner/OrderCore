using FluentAssertions;
using OrderCore.Domain.Entities;
using System;

namespace OrderCore.UnitTests.Domain
{
    public class ProductTests
    {
        [Fact]
        public void Should_Create_Product_With_Valid_Data()
        {
            // Arrange
            var name = "Notebook Dell";
            var price = 4500m;
            var stockQuantity = 10;

            // Act
            var product = new Product(name, price, stockQuantity);

            // Assert
            product.Name.Should().Be(name);
            product.Price.Should().Be(price);
            product.StockQuantity.Should().Be(stockQuantity);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Should_Throw_When_Price_Is_Invalid(decimal invalidPrice)
        {
            // Arrange
            Action action = () => new Product("Notebook Dell", invalidPrice, 10);

            // Act / Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*price*");
        }

        [Fact]
        public void Should_Increase_Stock_Correctly()
        {
            // Arrange
            var product = new Product("Notebook Dell", 4500m, 10);

            // Act
            product.IncreaseStock(5);

            // Assert
            product.StockQuantity.Should().Be(15);
        }

        [Fact]
        public void Should_Decrease_Stock_Correctly()
        {
            // Arrange
            var product = new Product("Notebook Dell", 4500m, 10);

            // Act
            product.DecreaseStock(3);

            // Assert
            product.StockQuantity.Should().Be(7);
        }

        [Fact]
        public void Should_Throw_When_Decreasing_More_Than_Available_Stock()
        {
            // Arrange
            var product = new Product("Notebook Dell", 4500m, 2);

            // Act
            Action action = () => product.DecreaseStock(3);

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*stock*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Should_Throw_When_Increasing_Invalid_Quantity(int invalidQuantity)
        {
            // Arrange
            var product = new Product("Notebook Dell", 4500m, 10);

            // Act
            Action action = () => product.IncreaseStock(invalidQuantity);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Should_Throw_When_Decreasing_Invalid_Quantity(int invalidQuantity)
        {
            // Arrange
            var product = new Product("Notebook Dell", 4500m, 10);

            // Act
            Action action = () => product.DecreaseStock(invalidQuantity);

            // Assert
            action.Should().Throw<ArgumentException>();
        }
    }
}