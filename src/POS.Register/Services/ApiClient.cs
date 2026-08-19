using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using POS.Register.Store;

namespace POS.Register.Services;

public class ApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;
    private readonly SessionStore _session;

    public event Action? SessionExpired;

    public ApiClient(AppConfig config, SessionStore session)
    {
        _session = session;
        _http = new HttpClient
        {
            BaseAddress = new Uri(config.ApiBaseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public Task<T> GetAsync<T>(string path) => SendAsync<T>(HttpMethod.Get, path, null);

    public Task<T> PostAsync<T>(string path, object body) => SendAsync<T>(HttpMethod.Post, path, body);

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body)
    {
        using var request = new HttpRequestMessage(method, path);
        if (_session.Token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.Token);
        }
        if (body is not null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(body, Json), Encoding.UTF8, "application/json");
        }

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ApiException("Can't reach the store service — is it running?", 0);
        }

        using (response)
        {
            var payload = await response.Content.ReadAsStringAsync();
            var statusCode = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                if (statusCode == 401)
                {
                    SessionExpired?.Invoke();
                }
                throw new ApiException(ReadError(payload, statusCode), statusCode);
            }
            return JsonSerializer.Deserialize<T>(payload, Json)
                ?? throw new ApiException("Empty response from the store service.", statusCode);
        }
    }

    private static string ReadError(string payload, int statusCode)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("error", out var error)
                && error.GetString() is { Length: > 0 } message)
            {
                return message;
            }
        }
        catch (JsonException)
        {
        }
        return $"The store service returned an error ({statusCode}).";
    }
}
