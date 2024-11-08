using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using SnowyRiver.WPF.NotifyPropertyChangedBase;

namespace SnowyRiver.WPF.Configuration;
public abstract class JsonConfiguration:NotifyPropertyChangedObject
{
    [JsonIgnore] 
    public string Path { get; set; } = string.Empty;

    protected virtual string DefaultPath => $"Configs/{GetType().Name}.json";

    [JsonIgnore] 
    public JsonConfigurationOptions Options { get; set; } = new();

    protected virtual JsonConfigurationOptions DefaultOptions => GlobalOptions;

    public static JsonConfigurationOptions GlobalOptions { get; set; } = new();

    [JsonIgnore]
    public string Json => JsonSerializer.Serialize(this, GetType());

    public event Action<JsonConfiguration>? Reading;
    public event Action<JsonConfiguration>? Creating;
    public event Action<JsonConfiguration>? Loaded;
    public event Action<JsonConfiguration>? BeforeSave;
    public event Action<JsonConfiguration>? AfterSave;

    protected virtual void OnReading() => Reading?.Invoke(this);

    protected virtual void OnCreating() => Creating?.Invoke(this);

    protected virtual void OnLoaded() => Loaded?.Invoke(this);

    protected virtual void OnBeforeSave() => BeforeSave?.Invoke(this);

    protected virtual void OnAfterSave() => AfterSave?.Invoke(this);

    public static T? Load<T>() where T : JsonConfiguration, new() => Load<T>(new T().DefaultPath, new T().DefaultOptions);
    public static T? Load<T>(string path) where T : JsonConfiguration, new() => Load<T>(path, new T().DefaultOptions);
    public static T? Load<T>(JsonConfigurationOptions options) where T : JsonConfiguration, new() => Load<T>(new T().DefaultPath, options);

    public static T? Load<T>(string path, JsonConfigurationOptions options) where T : JsonConfiguration, new()
    {
        T? config = default;
        if (File.Exists(path))
            config = Read<T>(path, options);
        else if (options.CreateNew)
            config = Create<T>(path, options);
        config?.OnLoaded();
        return config;
    }

    public static T? Read<T>() where T : JsonConfiguration, new() => Read<T>(new T().DefaultPath, new T().DefaultOptions);

    public static T? Read<T>(string path) where T : JsonConfiguration, new() => Read<T>(path, new T().DefaultOptions);

    public static T? Read<T>(JsonConfigurationOptions options) where T : JsonConfiguration, new() => Read<T>(new T().DefaultPath, options);

    public static T? Read<T>(string path, JsonConfigurationOptions options) where T : JsonConfiguration, new()
    {
        string json = File.ReadAllText(path);
        T? config = JsonSerializer.Deserialize<T>(json, options.SerializerOptions);
        if (config != null)
        {
            config.OnReading();
            config.Path = path;
            config.Options = options;
        }
        return config;
    }

    public static T Create<T>() where T : JsonConfiguration, new() => Create<T>(new T().DefaultPath, new T().DefaultOptions);

    public static T Create<T>(string path) where T : JsonConfiguration, new() => Create<T>(path, new T().DefaultOptions);

    public static T Create<T>(JsonConfigurationOptions options) where T : JsonConfiguration, new() => Create<T>(new T().DefaultPath, options);

    public static T Create<T>(string path, JsonConfigurationOptions options) where T : JsonConfiguration, new()
    {
        T config = new();
        config.OnCreating();
        config.Path = path;
        config.Options = options;
        if (options.SaveNew)
            config.Save();
        return config;
    }

    public void Save() => Save(Path, Options);
    public void Save(string path) => Save(path, Options);
    public void Save(JsonConfigurationOptions options) => Save(Path, options);
    public void Save(string path, JsonConfigurationOptions options)
    {
        OnBeforeSave();
        string json = JsonSerializer.Serialize(this, GetType(), options.SerializerOptions);
        File.WriteAllText(path, json);
        OnAfterSave();
    }

    public override string ToString() => Json;
}
