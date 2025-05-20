using System.Threading;
using System.Threading.Tasks;
using SnowyRiver.Reflection;

namespace SnowyRiver.Products;
public class ProductInfosProvider : IProductInfosProvider
{
    public virtual async Task<ProductInfo> GetProductInfosAsync(CancellationToken cancellationToken = default)
    {
        var result = new ProductInfo();
        var productInfo =  await Task.Run(ReflectionHelper.GetProductInfo, cancellationToken);
        result.Name = productInfo.Name;
        result.Version = productInfo.Version;
        return result;
    }
}
