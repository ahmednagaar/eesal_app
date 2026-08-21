using System.IO;
using Newtonsoft.Json;

namespace ReceiptSystem.Desktop.Services;

public static class AppSettings
{
    // In-memory default — only used before Load() runs on first startup.
    // Once a settings file exists, this is overwritten by the saved value.
    private const string DefaultUrl = "http://localhost:5000/api";

    public static string ApiBaseUrl { get; set; } = DefaultUrl;

    /// <summary>
    /// Returns true if the user has explicitly configured a server address
    /// (i.e., it's not the default localhost placeholder and not empty).
    /// </summary>
    public static bool HasConfiguredServer()
    {
        return !string.IsNullOrWhiteSpace(ApiBaseUrl)
            && !string.Equals(ApiBaseUrl.TrimEnd('/'), DefaultUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
    }

    private static string GetSettingsFilePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "ReceiptSystemDesktop");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        return Path.Combine(dir, "settings.json");
    }

    public static void Load()
    {
        var path = GetSettingsFilePath();
        if (File.Exists(path))
        {
            try
            {
                var json = File.ReadAllText(path);
                var settings = JsonConvert.DeserializeObject<dynamic>(json);
                var loaded = settings?.ApiBaseUrl?.ToString();

                // Only override the default if the loaded value is non-empty
                if (!string.IsNullOrWhiteSpace(loaded))
                {
                    ApiBaseUrl = loaded!;
                }
            }
            catch
            {
                // Ignore load errors, use default
            }
        }
    }

    public static void Save()
    {
        var path = GetSettingsFilePath();
        try
        {
            var json = JsonConvert.SerializeObject(new { ApiBaseUrl });
            File.WriteAllText(path, json);
        }
        catch
        {
            // Ignore save errors
        }
    }
}
