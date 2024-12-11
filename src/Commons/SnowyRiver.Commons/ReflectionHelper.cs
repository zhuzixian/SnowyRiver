using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace SnowyRiver.Commons;

public static class ReflectionHelper
{
    public static IEnumerable<Assembly> GetAssembliesByProductName(string productName)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in assemblies)
        {
            var asmCompanyAttr = asm.GetCustomAttribute<AssemblyProductAttribute>();
            if (asmCompanyAttr != null && asmCompanyAttr.Product == productName)
            {
                yield return asm;
            }
        }
    }

    public static ProductInfo GetProductInfo()
    {
        var productInfo = new ProductInfo();
        var assembly = Assembly.GetEntryAssembly();
        if (assembly != null)
        {
            productInfo.Name = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
            productInfo.Version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?.Split('+').FirstOrDefault();
        }

        return productInfo;
    }
}
