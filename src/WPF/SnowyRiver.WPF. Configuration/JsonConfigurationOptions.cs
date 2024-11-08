using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace SnowyRiver.WPF.Configuration;
public class JsonConfigurationOptions
{
    public JsonConfigurationOptions() { }

    public JsonConfigurationOptions(JsonConfigurationOptions options)
    {
        CreateNew = options.CreateNew;
        SaveNew = options.SaveNew;
        SerializerOptions = options.SerializerOptions;
    }

    /// <summary>
    /// Gets or sets a <see langword="bool"/> indicating whether to create and return a new config if the config file does not exist.
    /// </summary>
    public bool CreateNew { get; set; } = true;

    /// <summary>
    /// Gets or sets a <see langword="bool"/> indicating whether to save the newly created config immediately.
    /// </summary>
    public bool SaveNew { get; set; } = true;

    public JsonSerializerOptions SerializerOptions { get; set; } = new()
    {
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() },
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals | JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true
    };
}
