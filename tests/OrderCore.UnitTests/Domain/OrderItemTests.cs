using FluentAssertions;
using OrderCore.Domain.Entities;
using System;

namespace OrderCore.UnitTests.Domain
{
    public class OrderItemTests
    {
        [Fact]
        public void Should_Create_OrderItem_With_Valid_Data()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var productName = "Notebook Dell";
            var unitPrice = 4500m;
            var quantity = 2;

            // Act
            var orderItem = new OrderItem(productId, productName, unitPrice, quantity);

            // Assert
            orderItem.ProductId.Should().Be(productId);
            orderItem.ProductName.Should().Be(productName);
            orderItem.UnitPrice.Should().Be(unitPrice);
            orderItem.Quantity.Should().Be(quantity);
            orderItem.Total.Should().Be(9000m);
        }

        [Fact]
        public void Should_Throw_When_ProductId_Is_Empty()
        {
            // Act
            Action action = () => new OrderItem(Guid.Empty, "Notebook Dell", 4500m, 1);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*ProductId*");
        }

        [Fact]
        public void Should_Throw_When_ProductName_Is_Empty()
        {
            // Act
            Action action = () => new OrderItem(Guid.NewGuid(), "", 4500m, 1);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*Product name*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Should_Throw_When_UnitPrice_Is_Invalid(decimal invalidPrice)
        {
            // Act
            Action action = () => new OrderItem(Guid.NewGuid(), "Notebook Dell", invalidPrice, 1);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*Unit price*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_Throw_When_Quantity_Is_Invalid(int invalidQuantity)
        {
            // Act
            Action action = () => new OrderItem(Guid.NewGuid(), "Notebook Dell", 4500m, invalidQuantity);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*Quantity*");
        }

        [Fact]
        public void Should_Calculate_Total_Correctly()
        {
            // Arrange
            var orderItem = new OrderItem(Guid.NewGuid(), "Mouse Logitech", 150m, 3);

            // Act
            var total = orderItem.Total;

            // Assert
            total.Should().Be(450m);
        }
    }
}