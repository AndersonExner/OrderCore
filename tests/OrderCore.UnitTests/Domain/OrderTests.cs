using FluentAssertions;
using OrderCore.Domain.Entities;
using OrderCore.Domain.Enums;
using System;

namespace OrderCore.UnitTests.Domain
{
    public class OrderTests
    {
        [Fact]
        public void Should_Create_Order_With_Pending_Status()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            // Act
            var order = new Order(customerId);

            // Assert
            order.CustomerId.Should().Be(customerId);
            order.Status.Should().Be(OrderStatus.Pending);
            order.Items.Should().BeEmpty();
            order.TotalAmount.Should().Be(0);
        }

        [Fact]
        public void Should_Throw_When_CustomerId_Is_Empty()
        {
            // Act
            Action action = () => new Order(Guid.Empty);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*CustomerId*");
        }

        [Fact]
        public void Should_Add_Item_Correctly()
        {
            // Arrange
            var order = new Order(Guid.NewGuid());
            var productId = Guid.NewGuid();

            // Act
            order.AddItem(productId, "Notebook Dell", 4500m, 2);

            // Assert
            order.Items.Should().HaveCount(1);
            order.TotalAmount.Should().Be(9000m);

            var item = order.Items.First();
            item.ProductId.Should().Be(productId);
            item.ProductName.Should().Be("Notebook Dell");
            item.UnitPrice.Should().Be(4500m);
            item.Quantity.Should().Be(2);
            item.Total.Should().Be(9000m);
        }

        [Fact]
        public void Should_Calculate_TotalAmount_Correctly_With_Multiple_Items()
        {
            // Arrange
            var order = new Order(Guid.NewGuid());

            // Act
            order.AddItem(Guid.NewGuid(), "Notebook Dell", 4500m, 1);
            order.AddItem(Guid.NewGuid(), "Mouse Logitech", 150m, 2);

            // Assert
            order.TotalAmount.Should().Be(4800m);
        }

        [Fact]
        public void Should_Throw_When_Adding_Duplicate_Product()
        {
            // Arrange
            var order = new Order(Guid.NewGuid());
            var productId = Guid.NewGuid();

            order.AddItem(productId, "Notebook Dell", 4500m, 1);

            // Act
            Action action = () => order.AddItem(productId, "Notebook Dell", 4500m, 1);

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public void Should_Mark_Order_As_Paid()
        {
            // Arrange
            var order = new Order(Guid.NewGuid());
            order.AddItem(Guid.NewGuid(), "Notebook Dell", 4500m, 1);

            // Act
            order.MarkAsPaid();

            // Assert
            order.Status.Should().Be(OrderStatus.Paid);
        }

        [Fact]
        public void Should_Throw_When_Marking_Order_As_Paid_Without_Items()
        {
            // Arrange
            var order = new Order(Guid.NewGuid());

            // Act
            Action action = () => order.MarkAsPaid();

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*without items*");
        }

        [Fact]
        public void Should_Cancel_Order_When_Pending()
        {
            // Arrange
            var order = new Order(Guid.NewGuid());
            order.AddItem(Guid.NewGuid(), "Notebook Dell", 4500m, 1);

            // Act
            order.Cancel();

            // Assert
            order.Status.Should().Be(OrderStatus.Cancelled);
        }

        [Fact]
        public void Should_Not_Cancel_Paid_Order()
        {
            // Arrange
            var order = new Order(Guid.NewGuid());
            order.AddItem(Guid.NewGuid(), "Notebook Dell", 4500m, 1);
            order.MarkAsPaid();

            // Act
            Action action = () => order.Cancel();

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*cannot be cancelled*");
        }

        [Fact]
        public void Should_Not_Add_Item_When_Order_Is_Not_Pending()
        {
            // Arrange
            var order = new Order(Guid.NewGuid());
            order.AddItem(Guid.NewGuid(), "Notebook Dell", 4500m, 1);
            order.MarkAsPaid();

            // Act
            Action action = () => order.AddItem(Guid.NewGuid(), "Mouse Logitech", 150m, 1);

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*pending*");
        }
    }
}