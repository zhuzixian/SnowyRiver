using System.Threading;
using System.Threading.Tasks;
using SnowyRiver.Products.Abstractions;
using SnowyRiver.Reflection;

namespace SnowyRiver.Products;
public class ProductInfosProvider : IProductInfosProvider
{
    public virtual Task<ProductInfo> GetProductInfosAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(GetProductInfos, cancellationToken);
    }

    public virtual ProductInfo GetProductInfos()
    {
        var result = new ProductInfo();
        var productInfo = ReflectionHelper.GetProductInfo();
        result.Name = productInfo.Name;
        result.Version = $"{VersionPrefix}{productInfo.Version}";
        return result;
    }

    public virtual string VersionPrefix => "Ver.";
}
