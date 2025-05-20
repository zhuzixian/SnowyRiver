using System.Threading;
using System.Threading.Tasks;

namespace SnowyRiver.Products;
public interface IProductInfosProvider
{
    Task<ProductInfo> GetProductInfosAsync(CancellationToken cancellationToken = default);
    ProductInfo GetProductInfos();
}
