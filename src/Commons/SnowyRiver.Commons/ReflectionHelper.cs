using System.Collections.Generic;
using System.Reflection;
using System;

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
}
