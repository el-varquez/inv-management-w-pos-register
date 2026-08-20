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

    public async Task<T> GetAsync<T>(string path)
        => Deserialize<T>(await SendRawAsync(HttpMethod.Get, path, null));

    public async Task<T?> GetOptionalAsync<T>(string path) where T : class
    {
        var payload = await SendRawAsync(HttpMethod.Get, path, null);
        return string.IsNullOrWhiteSpace(payload) || payload == "null"
            ? null
            : JsonSerializer.Deserialize<T>(payload, Json);
    }

    public async Task<T> PostAsync<T>(string path, object body)
        => Deserialize<T>(await SendRawAsync(HttpMethod.Post, path, body));

    public async Task PostAsync(string path, object? body = null)
        => await SendRawAsync(HttpMethod.Post, path, body);

    public async Task PostAsync(string path, object? body, string bearerToken)
        => await SendRawAsync(HttpMethod.Post, path, body, bearerToken);

    private static T Deserialize<T>(string payload)
        => JsonSerializer.Deserialize<T>(payload, Json)
            ?? throw new ApiException("Empty response from the store service.", 200);

    private async Task<string> SendRawAsync(
        HttpMethod method, string path, object? body, string? bearerToken = null)
    {
        using var request = new HttpRequestMessage(method, path);
        var token = bearerToken ?? _session.Token;
        if (token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
                if (statusCode == 401 && bearerToken is null && _session.Token is not null)
                {
                    SessionExpired?.Invoke();
                }
                throw new ApiException(ReadError(payload, statusCode), statusCode);
            }
            return payload;
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
