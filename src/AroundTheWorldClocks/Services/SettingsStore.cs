using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AroundTheWorldClocks.Models;

namespace AroundTheWorldClocks.Services;

/// <summary>
/// Loads and saves <see cref="WidgetSettings"/> as JSON in %APPDATA%\AroundTheWorldClocks\settings.json.
/// </summary>
public class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public string FilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AroundTheWorldClocks",
        "settings.json");

    public WidgetSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var settings = JsonSerializer.Deserialize<WidgetSettings>(File.ReadAllText(FilePath), JsonOptions);
                if (settings is not null)
                    return settings;
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // Corrupt or unreadable file: fall back to defaults rather than crash the widget.
        }

        return WidgetSettings.CreateDefault();
    }

    public void Save(WidgetSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, JsonOptions));
    }
}
