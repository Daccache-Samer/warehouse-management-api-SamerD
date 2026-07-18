using AutoMapper;
using MediatR;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Commands.AddProductImage;

public class AddProductImageHandler(IProductRepository productRepository, IFileStorage fileStorage,IMapper mapper)
    : IRequestHandler<AddProductImageCommand, ProductViewModel>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];
    private const long MaxSizeBytes = 2 * 1024 * 1024; // 2 MB

    public async Task<ProductViewModel> Handle(AddProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
                      ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new ValidationException("Only .jpg, .jpeg, and .png files are allowed.");
        }

        if (request.Length > MaxSizeBytes)
        {
            throw new ValidationException("File size must not exceed 2 MB.");
        }

        var filePath = await fileStorage.SaveAsync(product.Id, request.Content, request.FileName, cancellationToken);
        var fileName = Path.GetFileName(filePath);

        var image = ProductImage.Create(product.Id, fileName, filePath);
        product.AddImage(image); // throws DomainException if product archived

        await productRepository.UpdateAsync(product, cancellationToken);
        return mapper.Map<ProductViewModel>(product);
    }
}