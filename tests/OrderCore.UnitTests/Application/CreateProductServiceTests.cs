using FluentAssertions;
using Moq;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Products.Commands;
using OrderCore.Application.Products.Dtos;
using OrderCore.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OrderCore.UnitTests.Application
{
    public class CreateProductServiceTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly CreateProductService _service;

        public CreateProductServiceTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _service = new CreateProductService(_productRepositoryMock.Object);
        }

        [Fact]
        public async Task Should_Create_Product_Successfully()
        {
            // Arrange
            var request = new CreateProductRequest
            {
                Name = "Notebook Dell",
                Price = 4500m,
                StockQuantity = 10
            };

            _productRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _service.ExecuteAsync(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Id.Should().NotBeEmpty();
            response.Name.Should().Be(request.Name);
            response.Price.Should().Be(request.Price);
            response.StockQuantity.Should().Be(request.StockQuantity);

            _productRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_Call_Repository_With_Valid_Product_Data()
        {
            // Arrange
            var request = new CreateProductRequest
            {
                Name = "Mouse Logitech",
                Price = 150m,
                StockQuantity = 25
            };

            Product? capturedProduct = null;

            _productRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .Callback<Product, CancellationToken>((product, _) => capturedProduct = product)
                .Returns(Task.CompletedTask);

            // Act
            await _service.ExecuteAsync(request, CancellationToken.None);

            // Assert
            capturedProduct.Should().NotBeNull();
            capturedProduct!.Name.Should().Be(request.Name);
            capturedProduct.Price.Should().Be(request.Price);
            capturedProduct.StockQuantity.Should().Be(request.StockQuantity);
        }
    }
}