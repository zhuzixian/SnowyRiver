using Akavache;
using Akavache.Core;
using Splat.Builder;

namespace SnowyRiver.LocalStorage.Akavache;

public static class AkavacheBuilderExtensions
{
    public static IAppBuilder WithAkavacheCacheDatabase<T>(this IAppBuilder builder,
        Action<IAkavacheBuilder> configure, 
        string? applicationName = null,
        FileLocationOption fileLocationOption = FileLocationOption.Default)
        where T : ISerializer, new()
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        CacheDatabase.Initialize<T>(configure, applicationName, fileLocationOption);

        return builder;
    }
}
