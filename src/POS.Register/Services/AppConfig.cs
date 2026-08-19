using System.IO;
using System.Text.Json;

namespace POS.Register.Services;

public class AppConfig
{
    public string ApiBaseUrl { get; init; } = "http://localhost:5103/api";

    public static AppConfig Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
        {
            return new AppConfig();
        }
        try
        {
            return JsonSerializer.Deserialize<AppConfig>(
                File.ReadAllText(path),
                new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? new AppConfig();
        }
        catch (JsonException)
        {
            return new AppConfig();
        }
    }
}
