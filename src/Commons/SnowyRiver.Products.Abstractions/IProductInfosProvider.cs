namespace SnowyRiver.Products.Abstractions;

public interface IProductInfosProvider
{
    Task<ProductInfo> GetProductInfosAsync(CancellationToken cancellationToken = default);
    ProductInfo GetProductInfos();
}

