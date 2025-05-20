using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using AsmResolver;

namespace SnowyRiver.Reflection;

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

    /// <summary>
    /// loop through all assemblies
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<Assembly> GetAllReferencedAssemblies(bool skipSystemAssemblies = true)
    {
        var rootAssembly = Assembly.GetEntryAssembly();
        if (rootAssembly == null)
        {
            rootAssembly = Assembly.GetCallingAssembly();
        }
        var returnAssemblies = new HashSet<Assembly>(new AssemblyEquality());
        var loadedAssemblies = new HashSet<string>();
        var assembliesToCheck = new Queue<Assembly>();
        assembliesToCheck.Enqueue(rootAssembly);
        if (skipSystemAssemblies && IsSystemAssembly(rootAssembly))
        {
            if (IsValid(rootAssembly))
            {
                returnAssemblies.Add(rootAssembly);
            }
        }
        while (assembliesToCheck.Any())
        {
            var assemblyToCheck = assembliesToCheck.Dequeue();
            foreach (var reference in assemblyToCheck.GetReferencedAssemblies())
            {
                if (!loadedAssemblies.Contains(reference.FullName))
                {
                    var assembly = Assembly.Load(reference);
                    if (skipSystemAssemblies && IsSystemAssembly(assembly))
                    {
                        continue;
                    }
                    assembliesToCheck.Enqueue(assembly);
                    loadedAssemblies.Add(reference.FullName);
                    if (IsValid(assembly))
                    {
                        returnAssemblies.Add(assembly);
                    }
                }
            }
        }
        var assembliesInBaseDir = Directory.EnumerateFiles(AppContext.BaseDirectory,
            "*.dll", SearchOption.AllDirectories);
        foreach (var asmPath in assembliesInBaseDir)
        {
            if (!IsManagedAssembly(asmPath))
            {
                continue;
            }
            AssemblyName asmName = AssemblyName.GetAssemblyName(asmPath);
            //如果程序集已经加载过了就不再加载
            if (returnAssemblies.Any(x => AssemblyName.ReferenceMatchesDefinition(x.GetName(), asmName)))
            {
                continue;
            }
            if (skipSystemAssemblies && IsSystemAssembly(asmPath))
            {
                continue;
            }
            Assembly? asm = TryLoadAssembly(asmPath);
            if (asm == null)
            {
                continue;
            }
            //Assembly asm = Assembly.Load(asmName);
            if (!IsValid(asm))
            {
                continue;
            }
            if (skipSystemAssemblies && IsSystemAssembly(asm))
            {
                continue;
            }
            returnAssemblies.Add(asm);
        }
        return returnAssemblies.ToArray();
    }

    public static (string? Name, string? Version) GetProductInfo()
    {
        var productName = default(string?);
        var productVersion = default(string?);
        var assembly = Assembly.GetEntryAssembly();
        if (assembly != null)
        {
            productName = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
            productVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?.Split('+').FirstOrDefault();
        }

        return (productName, productVersion);
    }

    private static bool IsSystemAssembly(string asmPath)
    {
        var moduleDef = AsmResolver.DotNet.ModuleDefinition.FromFile(asmPath);
        var assembly = moduleDef.Assembly;
        if (assembly == null)
        {
            return false;
        }
        var asmCompanyAttr = assembly.CustomAttributes.FirstOrDefault(c => c.Constructor?.DeclaringType?.FullName == typeof(AssemblyCompanyAttribute).FullName);
        var companyName = ((Utf8String?)asmCompanyAttr?.Signature?.FixedArguments[0]?.Element)?.Value;
        return companyName != null && companyName.Contains("Microsoft");
    }

    //是否是微软等的官方Assembly
    private static bool IsSystemAssembly(Assembly asm)
    {
        var asmCompanyAttr = asm.GetCustomAttribute<AssemblyCompanyAttribute>();
        if (asmCompanyAttr == null)
        {
            return false;
        }
        else
        {
            string companyName = asmCompanyAttr.Company;
            return companyName.Contains("Microsoft");
        }
    }

    /// <summary>
    /// 判断file这个文件是否是程序集
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private static bool IsManagedAssembly(string file)
    {
        using var fs = File.OpenRead(file);
        using PEReader peReader = new PEReader(fs);
        return peReader.HasMetadata && peReader.GetMetadataReader().IsAssembly;
    }

    private static Assembly? TryLoadAssembly(string asmPath)
    {
        var asmName = AssemblyName.GetAssemblyName(asmPath);
        Assembly? asm = null;
        try
        {
            asm = Assembly.Load(asmName);
        }
        catch (BadImageFormatException ex)
        {
            Debug.WriteLine(ex);
        }
        catch (FileLoadException ex)
        {
            Debug.WriteLine(ex);
        }

        if (asm == null)
        {
            try
            {
                asm = Assembly.LoadFile(asmPath);
            }
            catch (BadImageFormatException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (FileLoadException ex)
            {
                Debug.WriteLine(ex);
            }
        }
        return asm;
    }



    private static bool IsValid(Assembly asm)
    {
        try
        {
            asm.GetTypes();
            _ = asm.DefinedTypes.ToList();
            return true;
        }
        catch (ReflectionTypeLoadException)
        {
            return false;
        }
    }

    private class AssemblyEquality : EqualityComparer<Assembly>
    {
        public override bool Equals(Assembly? x, Assembly? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return AssemblyName.ReferenceMatchesDefinition(x.GetName(), y.GetName());
        }

        public override int GetHashCode(Assembly obj)
        {
            return obj.GetName().FullName.GetHashCode();
        }
    }
}
