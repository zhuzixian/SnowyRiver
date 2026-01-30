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

        if (fileLocationOption == FileLocationOption.Legacy && !string.IsNullOrWhiteSpace(applicationName))
        {
            var localMachineDirectory = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData), applicationName, "BlobCache");
            if (!string.IsNullOrWhiteSpace(localMachineDirectory) && !Directory.Exists(localMachineDirectory))
            {
                Directory.CreateDirectory(localMachineDirectory);
            }

            var userAccountDirectory = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData), applicationName, "BlobCache");
            if (!string.IsNullOrWhiteSpace(userAccountDirectory) && !Directory.Exists(userAccountDirectory))
            {
                Directory.CreateDirectory(userAccountDirectory);
            }

            var secureDirectory = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), applicationName, "SecretCache");
            if (!string.IsNullOrWhiteSpace(secureDirectory) && !Directory.Exists(secureDirectory))
            {
                Directory.CreateDirectory(secureDirectory);
            }
        }

        CacheDatabase.Initialize<T>(configure, applicationName, fileLocationOption);
        return builder;
    }
}
