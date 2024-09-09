using System;
using System.ComponentModel;
using System.Resources;

namespace SnowyRiver.WPF.Localized;
public class LocalizedDescriptionAttribute(string resourceKey, Type resourceType) : DescriptionAttribute
{
    readonly ResourceManager _resourceManager = new(resourceType);

    public override string Description
    {
        get
        {
            var description = _resourceManager.GetString(resourceKey);
            return string.IsNullOrWhiteSpace(description) ? $"[[{resourceKey}]]" : description;
        }
    }
}

