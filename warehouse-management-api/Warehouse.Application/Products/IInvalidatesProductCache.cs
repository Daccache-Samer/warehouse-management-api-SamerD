namespace Warehouse.Application.Products;

/// <summary>
/// Implemented by commands that trigger product cache invalidation after a successful write.
/// ProductId is null for commands where no "by id" entry could exist yet (e.g. creation).
/// </summary>
public interface IInvalidatesProductCache
{
    string? ProductId { get; }
}