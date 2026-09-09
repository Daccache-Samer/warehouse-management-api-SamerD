using System.Text;
using AutoMapper;
using FluentAssertions;
using Moq;
using Warehouse.Api.UnitTests.TestUtilities.Builders;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Products.Commands.AddProductImage;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Common;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Api.UnitTests.ProductService;

public class AddProductImage
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IFileStorage> _fileStorageMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();

    private readonly AddProductImageHandler _sut;

    public AddProductImage()
    {
        var correlationContextMock = new Mock<ICorrelationContext>();
        _sut = new AddProductImageHandler(
            _productRepositoryMock.Object,
            _fileStorageMock.Object,
            _mapperMock.Object,
            _eventPublisherMock.Object,
            correlationContextMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidJpgFile_UploadsSuccessfullyAndReturnsViewModel()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        var product = new ProductBuilder().WithId(productId).Build();

        var stream = new MemoryStream(Encoding.UTF8.GetBytes("fake-image-bytes"));
        var command = new AddProductImageCommand(
            productId, stream, "photo.jpg", 1024, "image/jpeg");

        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _fileStorageMock.Setup(fs => fs.UploadAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileStorageResult("photo.jpg", "products/photo.jpg"));

        _mapperMock
            .Setup(m => m.Map<ProductViewModel>(product))
            .Returns(new ProductViewModel());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _fileStorageMock.Verify(fs => fs.UploadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(repo =>
            repo.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidPngFile_UploadsSuccessfullyAndReturnsViewModel()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        var product = new ProductBuilder().WithId(productId).Build();

        var stream = new MemoryStream(Encoding.UTF8.GetBytes("fake-image-bytes"));
        var command = new AddProductImageCommand(
            productId, stream, "photo.png", 1024, "image/png");

        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _fileStorageMock.Setup(fs => fs.UploadAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileStorageResult("photo.png", "products/photo.png"));

        _mapperMock
            .Setup(m => m.Map<ProductViewModel>(product))
            .Returns(new ProductViewModel());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _fileStorageMock.Verify(fs => fs.UploadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(repo =>
            repo.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidExtension_ThrowsValidationException()
    {
        // Arrange
        var command = new AddProductImageCommand(
            Guid.NewGuid().ToString(), Stream.Null, "document.txt", 1024, "text/plain");
        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductBuilder().Build());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Only .jpg, .jpeg, and .png files are allowed.");
    }

    [Fact]
    public async Task Handle_InvalidContentType_ThrowsValidationException()
    {
        // Arrange
        var command = new AddProductImageCommand(
            Guid.NewGuid().ToString(), Stream.Null, "photo.jpg", 1024, "application/pdf");
        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductBuilder().Build());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("File content type must be image/jpeg or image/png.");
    }

    [Fact]
    public async Task Handle_FileExceedsTwoMegabytes_ThrowsValidationException()
    {
        // Arrange
        const long oversizedLength = 2 * 1024 * 1024 + 1;
        var command = new AddProductImageCommand(
            Guid.NewGuid().ToString(), Stream.Null, "photo.jpg", oversizedLength, "image/jpeg");
        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductBuilder().Build());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("File size must not exceed 2 MB.");
    }
}