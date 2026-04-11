using FluentAssertions;
using OrderCore.Domain.Entities;
using System;

namespace OrderCore.UnitTests.Domain
{
    public class CustomerTests
    {
        [Fact]
        public void Should_Create_Customer_With_Valid_Data()
        {
            // Arrange
            var name = "test";
            var email = "test@email.com";

            // Act
            var customer = new Customer(name, email);

            // Assert
            customer.Name.Should().Be(name);
            customer.Email.Should().Be(email);
            customer.Id.Should().NotBeEmpty();
        }

        [Fact]
        public void Should_Throw_When_Name_Is_Empty()
        {
            // Act
            Action action = () => new Customer("", "test@email.com");

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*name*");
        }

        [Fact]
        public void Should_Throw_When_Email_Is_Empty()
        {
            // Act
            Action action = () => new Customer("test", "");

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*email*");
        }

        [Fact]
        public void Should_Throw_When_Email_Is_Invalid()
        {
            // Act
            Action action = () => new Customer("test", "invalid-email");

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*Invalid email*");
        }

        [Fact]
        public void Should_Update_Customer_With_Valid_Data()
        {
            // Arrange
            var customer = new Customer("test", "test@email.com");

            // Act
            customer.Update("test new", "test.silva@email.com");

            // Assert
            customer.Name.Should().Be("test new");
            customer.Email.Should().Be("test.silva@email.com");
        }

        [Fact]
        public void Should_Throw_When_Updating_With_Invalid_Name()
        {
            // Arrange
            var customer = new Customer("test", "test@email.com");

            // Act
            Action action = () => customer.Update("", "new@email.com");

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*name*");
        }

        [Fact]
        public void Should_Throw_When_Updating_With_Invalid_Email()
        {
            // Arrange
            var customer = new Customer("test", "test@email.com");

            // Act
            Action action = () => customer.Update("test", "invalid-email");

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*Invalid email*");
        }
    }
}