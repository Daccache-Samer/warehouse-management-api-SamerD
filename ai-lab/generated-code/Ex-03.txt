using FluentAssertions;
using Moq;
using Warehouse.Api.UnitTests.TestUtilities.Builders;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Products.Queries.DownloadProductImage;
using Warehouse.DomainWarehouse.Domain.Common;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Api.UnitTests.ProductService;

public class DownloadProductImageHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IFileStorage> _fileStorageMock = new();
    private readonly DownloadProductImageHandler _handler;

    public DownloadProductImageHandlerTests()
    {
        _handler = new DownloadProductImageHandler(_productRepositoryMock.Object, _fileStorageMock.Object);
    }

    private static ProductImage AttachImage(Product product, string fileName, string objectKey)
    {
        var image = ProductImage.Create(product.Id, fileName, objectKey);
        product.AddImage(image);
        return image;
    }

    // ---------------------------------------------------------------------
    // Positive tests
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Handle_ProductAndImageExist_ReturnsContentContentTypeAndFileName()
    {
        // Arrange
        var product = new ProductBuilder().Build();
        var image = AttachImage(product, "label.png", "products/{product.Id}/abc123.png");
        var content = new MemoryStream([1, 2, 3, 4]);
        var query = new DownloadProductImageQuery(product.Id, image.FileName);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _fileStorageMock
            .Setup(s => s.DownloadAsync(image.ObjectKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((content, "image/png"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Content.Should().BeSameAs(content);
        result.ContentType.Should().Be("image/png");
        result.FileName.Should().Be(image.FileName);
    }

    [Fact]
    public async Task Handle_ProductHasMultipleImages_ReturnsTheMatchingOneByFileName()
    {
        // Arrange
        var product = new ProductBuilder().Build();
        AttachImage(product, "front.png", "products/front-key.png");
        var target = AttachImage(product, "back.png", "products/back-key.png");
        AttachImage(product, "side.png", "products/side-key.png");

        var content = new MemoryStream([9, 9]);
        var query = new DownloadProductImageQuery(product.Id, "back.png");

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _fileStorageMock
            .Setup(s => s.DownloadAsync(target.ObjectKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((content, "image/png"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.FileName.Should().Be("back.png");
        _fileStorageMock.Verify(s => s.DownloadAsync(target.ObjectKey, It.IsAny<CancellationToken>()), Times.Once);
        // Confirms the non-matching images' object keys were never requested from storage.
        _fileStorageMock.Verify(s => s.DownloadAsync("products/front-key.png", It.IsAny<CancellationToken>()), Times.Never);
        _fileStorageMock.Verify(s => s.DownloadAsync("products/side-key.png", It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PropagatesCancellationTokenToRepositoryAndStorage()
    {
        // Arrange
        var product = new ProductBuilder().Build();
        var image = AttachImage(product, "doc.pdf", "products/doc-key.pdf");
        using var cts = new CancellationTokenSource();
        var token = cts.Token; // This is a manual refactor
        var query = new DownloadProductImageQuery(product.Id, image.FileName);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, token))
            .ReturnsAsync(product);
        _fileStorageMock
            .Setup(s => s.DownloadAsync(image.ObjectKey, token))
            .ReturnsAsync((new MemoryStream(), "application/pdf"));

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _productRepositoryMock.Verify(r => r.GetByIdAsync(product.Id, token), Times.Once);
        _fileStorageMock.Verify(s => s.DownloadAsync(image.ObjectKey, token), Times.Once);
    }

    // ---------------------------------------------------------------------
    // Negative tests
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Handle_ProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const string productId = "missing-product-id";
        var query = new DownloadProductImageQuery(productId, "anything.png");

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Product with id '{productId}' was not found.");
        _fileStorageMock.Verify(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProductHasNoImages_ThrowsNotFoundException()
    {
        // Arrange
        var product = new ProductBuilder().Build(); // no images attached
        var query = new DownloadProductImageQuery(product.Id, "label.png");

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Image 'label.png' was not found on this product.");
        _fileStorageMock.Verify(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProductHasImagesButNoneMatchRequestedFileName_ThrowsNotFoundException()
    {
        // Arrange
        var product = new ProductBuilder().Build();
        AttachImage(product, "front.png", "products/front-key.png");
        AttachImage(product, "back.png", "products/back-key.png");
        var query = new DownloadProductImageQuery(product.Id, "does-not-exist.png");

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Image 'does-not-exist.png' was not found on this product.");
    }

    [Fact]
    public async Task Handle_RepositoryThrows_ExceptionPropagatesUnchanged()
    {
        // Arrange
        const string productId = "some-id";
        var query = new DownloadProductImageQuery(productId, "label.png");
        var dbException = new TimeoutException("Database connection timed out.");

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbException);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert — handler has no try/catch, so ExceptionHandlingMiddleware is the only
        // place this should be caught; the handler must not swallow or rewrap it.
        (await act.Should().ThrowAsync<TimeoutException>()).Which.Should().BeSameAs(dbException);
    }

    [Fact]
    public async Task Handle_FileStorageThrows_ExceptionPropagatesUnchanged()
    {
        // Arrange
        var product = new ProductBuilder().Build();
        var image = AttachImage(product, "label.png", "products/label-key.png");
        var query = new DownloadProductImageQuery(product.Id, image.FileName);
        var storageException = new InvalidOperationException("MinIO endpoint unreachable.");

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _fileStorageMock
            .Setup(s => s.DownloadAsync(image.ObjectKey, It.IsAny<CancellationToken>()))
            .ThrowsAsync(storageException);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert — this is the "DB says the image exists but the object is gone/unreachable
        // in MinIO" drift scenario; the handler should not mask it as a 404.
        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(storageException);
    }

    // ---------------------------------------------------------------------
    // Edge cases
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Handle_FileNameDiffersOnlyByCase_IsTreatedAsNotFound()
    {
        // Arrange — documents current behavior: FileName comparison uses default (ordinal,
        // case-sensitive) string equality. Flagging this: if uploads ever normalize
        // casing differently than lookups (e.g. client-side vs. stored FileName), this
        // will silently 404 instead of matching. Worth confirming this is intentional.
        var product = new ProductBuilder().Build();
        AttachImage(product, "Report.pdf", "products/report-key.pdf");
        var query = new DownloadProductImageQuery(product.Id, "report.pdf");

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Image 'report.pdf' was not found on this product.");
    }

    [Fact]
    public async Task Handle_StorageReturnsZeroByteStream_ReturnsEmptyContentWithoutThrowing()
    {
        // Arrange
        var product = new ProductBuilder().Build();
        var image = AttachImage(product, "empty.bin", "products/empty-key.bin");
        var emptyStream = new MemoryStream(Array.Empty<byte>());
        var query = new DownloadProductImageQuery(product.Id, image.FileName);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _fileStorageMock
            .Setup(s => s.DownloadAsync(image.ObjectKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((emptyStream, "application/octet-stream"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Content.Length.Should().Be(0);
        result.ContentType.Should().Be("application/octet-stream");
    }

    [Fact]
    public async Task Handle_VeryLongFileName_MatchesAndReturnsSuccessfully()
    {
        // Arrange — boundary check for unusually long file names (well beyond typical
        // filesystem limits) reaching this far without truncation issues in the handler.
        var longFileName = new string('a', 500) + ".png";
        var product = new ProductBuilder().Build();
        var image = AttachImage(product, longFileName, "products/long-name-key.png");
        var content = new MemoryStream([5, 5, 5]);
        var query = new DownloadProductImageQuery(product.Id, longFileName);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _fileStorageMock
            .Setup(s => s.DownloadAsync(image.ObjectKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((content, "image/png"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.FileName.Should().Be(longFileName);
        result.FileName.Length.Should().Be(504);
    }
}