using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using SnowyRiver.Commons;
using WPFLocalizeExtension.Extensions;

namespace SnowyRiver.WPF.Localized;

public class LocalizationProvider
{
    public LocalizationProvider()
    {
    }

    public LocalizationProvider(IEnumerable<Assembly> assemblies)
    {
        Assemblies = assemblies;
    }

    protected readonly IEnumerable<Assembly> Assemblies = [];

    public string GetLocalizedValue(string key)
    {
        var assemblies = Assemblies.ToArray();
        if (!assemblies.Any())
        {
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }
        foreach (var assembly in assemblies)
        {
            var value = GetLocalizedValue(key, assembly);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    public static string GetLocalizedValueFromCurrentAssembly(string key)
    {
        var stackTrace = new StackTrace();
        var frame = stackTrace.GetFrame(1);
        var assembly = frame?.GetMethod()?.DeclaringType?.Assembly;
        if (assembly != null)
        {
            return GetLocalizedValue(key, assembly);
        }

        return string.Empty;
    }

    public static string GetLocalizedValue(string key, Assembly assembly)
    {
        return LocExtension.GetLocalizedValue<string>(assembly.GetName().Name + ":Resources:" + key);
    }
}