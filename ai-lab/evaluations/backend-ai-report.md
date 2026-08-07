**Best Prompt**: Context: I am working on a warehouse management API project using a DDD architecture and CQRS. I am using PostgreSQL for the database, Redis for caching, Serilog for logging, RabbitMQ for intercommunication between microservices, Minio for object storage, and xUnit, Moq, and Fluent Assertions for testing. A configuration defect exists in my AssignSupplierToProductHandler. Currently, historical or archived product entities can still be mapped to live, active suppliers.
Role: Senior Software Engineer.
Task: Generate the required implementation for this endpoint:

*  A clear plain-text explanation of the faulty logic sequence.
*  The precise line numbers or blocks containing the logical oversight.
*  A robust, high-performance C# fix conforming to domain constraints.
*  An isolated unit test verifying that mapping a supplier to an archived product safely throws an exception.

If you have any questions about the implementation, project, or anything related to them, ask before generating anything.

**Best Generated Code**: 

## Faulty logic sequence


```csharp
var supplier = await supplierRepository.GetByIdAsync(request.SupplierId, cancellationToken)
               ?? throw new NotFoundException($"Supplier with id '{request.SupplierId}' was not found.");
// <-- missing call to product.AssignSupplier(supplier) here
await productRepository.UpdateAsync(product, cancellationToken);
```

## Fix

```csharp
public class AssignSupplierToProductHandler(
    IProductRepository productRepository, ISupplierRepository supplierRepository, IMapper mapper, IDistributedCache cache)
    : IRequestHandler<AssignSupplierToProductCommand, ProductViewModel>
{
    public async Task<ProductViewModel> Handle(AssignSupplierToProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
                      ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        var supplier = await supplierRepository.GetByIdAsync(request.SupplierId, cancellationToken)
                       ?? throw new NotFoundException($"Supplier with id '{request.SupplierId}' was not found.");

        product.AssignSupplier(supplier); // enforces EnsureNotArchived() + supplier.IsActive

        await productRepository.UpdateAsync(product, cancellationToken);
        await cache.RemoveAsync($"GetProductByIdQuery-{product.Id}", cancellationToken);
        await cache.RemoveAsync("ListProductsHandler_ListProductsQuery", cancellationToken);

        return mapper.Map<ProductViewModel>(product);
    }
}
```

## Unit test

```csharp
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Warehouse.Application.Products.Commands.AssignSupplierToProduct;
using Warehouse.DomainWarehouse.Domain.Exceptions;
using Warehouse.DomainWarehouse.Domain.Products;
using Warehouse.DomainWarehouse.Domain.Suppliers;
using Xunit;

namespace Warehouse.Api.UnitTests.Products.Commands;

public class AssignSupplierToProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<ISupplierRepository> _supplierRepository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IDistributedCache> _cache = new();

    private AssignSupplierToProductHandler CreateSut() =>
        new(_productRepository.Object, _supplierRepository.Object, _mapper.Object, _cache.Object);

    [Fact]
    public async Task Handle_ArchivedProduct_ThrowsDomainException_AndDoesNotPersist()
    {
        // Arrange
        var product = Product.Create("Widget", "SKU-001", "desc", 10m, 5, DateTime.UtcNow.AddDays(30));
        product.Archive();

        var supplier = Supplier.Create("Acme", "US", "a@acme.com", "1234567890");

        _productRepository
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _supplierRepository
            .Setup(r => r.GetByIdAsync(supplier.SupplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplier);

        var sut = CreateSut();
        var command = new AssignSupplierToProductCommand(product.Id, supplier.SupplierId);

        // Act
        Func<Task> act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Archived products cannot be updated.");

        _productRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _cache.Verify(
            c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

```

**Incorrect Generated Code**:

```csharp
using FluentAssertions;
using Moq;
using Warehouse.Api.UnitTests.TestUtilities.Builders;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Products.Queries.DownloadProductImage;
using Warehouse.DomainWarehouse.Domain.Common;
using Warehouse.DomainWarehouse.Domain.Products;
using Xunit;

namespace Warehouse.Api.UnitTests.Products.Queries;

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
        var query = new DownloadProductImageQuery(product.Id, image.FileName);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, cts.Token))
            .ReturnsAsync(product);
        _fileStorageMock
            .Setup(s => s.DownloadAsync(image.ObjectKey, cts.Token))
            .ReturnsAsync((new MemoryStream(), "application/pdf"));

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _productRepositoryMock.Verify(r => r.GetByIdAsync(product.Id, cts.Token), Times.Once);
        _fileStorageMock.Verify(s => s.DownloadAsync(image.ObjectKey, cts.Token), Times.Once);
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

```

**Human Corrections**:

```csharp

using var cts = new CancellationTokenSource();
_productRepositoryMock
.Setup(r => r.GetByIdAsync(product.Id, cts.Token))
.ReturnsAsync(product);
_fileStorageMock
.Setup(s => s.DownloadAsync(image.ObjectKey, cts.Token))
.ReturnsAsync((new MemoryStream(), "application/pdf"));


var token = cts.Token;
_productRepositoryMock
.Setup(r => r.GetByIdAsync(product.Id, token))
.ReturnsAsync(product);
_fileStorageMock
.Setup(s => s.DownloadAsync(image.ObjectKey, token))
.ReturnsAsync((new MemoryStream(), "application/pdf"));

```
We were capturing an outer variable inside of the lambda expressions wich was generating an error. So I created a token variable that contains cts.Token instead of passing it directly in the lambdas.

**Lessons Learned**:
As developers we can us AI agents and tools to generate code and fix issues faster but we still need to check the output before implementing it because it may contain errors or incompatibilities with the rest of our codebase.
