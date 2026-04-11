using SnowyRiver.Configuration;

namespace SnowyRiver.WPF.Prism.JsonConfigs;

public static class IContainerRegistryExtensions
{
    public static IContainerRegistry RegisterJsonConfig<T>(
        this IContainerRegistry containerRegistry,
        string fileName)
        where T: JsonConfiguration, new()
    {
        var options =  JsonConfiguration.Load<T>(fileName);
        containerRegistry.RegisterInstance(options);
        return containerRegistry;
    }
}
