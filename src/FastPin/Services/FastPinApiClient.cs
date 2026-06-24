using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FastPin.Models;

namespace FastPin.Services;

public class FastPinApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AppSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private string? _accessToken;

    public FastPinApiClient()
    {
        _settings = AppSettings.Load();
        var baseUrl = _settings.ApiBaseUrl;
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "https://localhost:5001";
        }

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/")
        };
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (!string.IsNullOrWhiteSpace(_accessToken))
        {
            return;
        }

        var username = string.IsNullOrWhiteSpace(_settings.ApiUsername) ? "admin" : _settings.ApiUsername;
        var password = string.IsNullOrWhiteSpace(_settings.ApiPassword) ? "change-me" : _settings.ApiPassword;

        var response = await _httpClient.PostAsJsonAsync("api/auth/login", new
        {
            Username = username,
            Password = password
        });

        response.EnsureSuccessStatusCode();

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
        _accessToken = login?.Token ?? throw new InvalidOperationException("Authentication token was not returned by API.");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    private async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> action)
    {
        await EnsureAuthenticatedAsync();

        var response = await action();
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        _accessToken = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
        await EnsureAuthenticatedAsync();
        return await action();
    }

    public async Task<List<PinnedItem>> GetPinnedItemsAsync()
    {
        var response = await SendAsync(() => _httpClient.GetAsync("api/pinneditems"));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<PinnedItem>>(_jsonOptions) ?? new List<PinnedItem>();
    }

    public async Task<PinnedItem> CreatePinnedItemAsync(PinnedItem item)
    {
        var response = await SendAsync(() => _httpClient.PostAsJsonAsync("api/pinneditems", item));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PinnedItem>(_jsonOptions))!;
    }

    public async Task DeletePinnedItemAsync(int id)
    {
        var response = await SendAsync(() => _httpClient.DeleteAsync($"api/pinneditems/{id}"));
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateFileCacheAsync(int id, bool isCached, byte[]? cachedFileData)
    {
        var response = await SendAsync(() => _httpClient.PutAsJsonAsync($"api/pinneditems/{id}/cache", new
        {
            IsCached = isCached,
            CachedFileData = cachedFileData
        }));
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<Tag>> GetTagsAsync()
    {
        var response = await SendAsync(() => _httpClient.GetAsync("api/tags"));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Tag>>(_jsonOptions) ?? new List<Tag>();
    }

    public async Task<Tag> EnsureTagAsync(string tagName)
    {
        var response = await SendAsync(() => _httpClient.PostAsJsonAsync("api/tags/ensure", new { Name = tagName }));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Tag>(_jsonOptions))!;
    }

    public async Task<Tag?> GetTagByNameAsync(string tagName)
    {
        var encodedTag = Uri.EscapeDataString(tagName);
        var response = await SendAsync(() => _httpClient.GetAsync($"api/tags/by-name/{encodedTag}"));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Tag>(_jsonOptions);
    }

    public async Task UpdateTagAsync(Tag tag)
    {
        var response = await SendAsync(() => _httpClient.PutAsJsonAsync($"api/tags/{tag.Id}", tag));
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTagAsync(int tagId)
    {
        var response = await SendAsync(() => _httpClient.DeleteAsync($"api/tags/{tagId}"));
        response.EnsureSuccessStatusCode();
    }

    public async Task AddTagToItemAsync(int itemId, int tagId)
    {
        var response = await SendAsync(() => _httpClient.PostAsync($"api/pinneditems/{itemId}/tags/{tagId}", null));
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveTagFromItemAsync(int itemId, int tagId)
    {
        var response = await SendAsync(() => _httpClient.DeleteAsync($"api/pinneditems/{itemId}/tags/{tagId}"));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    private sealed class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
