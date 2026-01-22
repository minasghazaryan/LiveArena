using LLiveArenaWeb.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LLiveArenaWeb.Services;

public class StreamService : IStreamService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string RapidApiHost = "all-sport-live-stream.p.rapidapi.com";
    private const string RapidApiKey = "46effbe6bcmshf54dd907f3cd18ap127fd8jsn9d2ba3ddee14";
    private const string BaseUrl = "https://all-sport-live-stream.p.rapidapi.com/api/d/stream_source";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private readonly object _cacheLock = new();
    private readonly Dictionary<long, (DateTime ExpiresAt, StreamResponse Response)> _cache = new();

    public StreamService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<StreamResponse?> GetStreamSourceAsync(long gmid)
    {
        try
        {
            lock (_cacheLock)
            {
                if (_cache.TryGetValue(gmid, out var cached) && cached.ExpiresAt > DateTime.UtcNow)
                {
                    return cached.Response;
                }
            }

            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{BaseUrl}?gmid={gmid}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-rapidapi-host", RapidApiHost);
            request.Headers.Add("x-rapidapi-key", RapidApiKey);
            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // Try to parse as StreamResponse first
                try
                {
                    var streamResponse = JsonSerializer.Deserialize<StreamResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    // If deserialization worked but StreamUrl is empty, try to extract from raw JSON
                    if (streamResponse != null && string.IsNullOrEmpty(streamResponse.Data?.StreamUrl))
                    {
                        using var doc = JsonDocument.Parse(content);
                        var root = doc.RootElement;
                        
                        // Try to find stream URL in various possible locations
                        if (root.TryGetProperty("data", out var dataElement))
                        {
                            if (dataElement.TryGetProperty("streamUrl", out var streamUrl) || 
                                dataElement.TryGetProperty("stream_url", out streamUrl) ||
                                dataElement.TryGetProperty("url", out streamUrl) ||
                                dataElement.TryGetProperty("source", out streamUrl))
                            {
                                streamResponse.Data ??= new StreamData();
                                streamResponse.Data.StreamUrl = streamUrl.GetString();
                                streamResponse.Success = true;
                            }
                        }
                        else if (root.TryGetProperty("streamUrl", out var directUrl) ||
                                 root.TryGetProperty("stream_url", out directUrl) ||
                                 root.TryGetProperty("url", out directUrl))
                        {
                            streamResponse.Data ??= new StreamData();
                            streamResponse.Data.StreamUrl = directUrl.GetString();
                            streamResponse.Success = true;
                        }
                    }
                    
                    if (streamResponse != null)
                    {
                        CacheResponse(gmid, streamResponse);
                    }
                    return streamResponse;
                }
                catch
                {
                    // If deserialization fails, try to extract URL directly from JSON
                    try
                    {
                        using var doc = JsonDocument.Parse(content);
                        var root = doc.RootElement;
                        string? streamUrl = null;
                        
                        if (root.TryGetProperty("data", out var dataElement))
                        {
                            if (dataElement.TryGetProperty("streamUrl", out var urlElement)) streamUrl = urlElement.GetString();
                            else if (dataElement.TryGetProperty("stream_url", out urlElement)) streamUrl = urlElement.GetString();
                            else if (dataElement.TryGetProperty("url", out urlElement)) streamUrl = urlElement.GetString();
                            else if (dataElement.TryGetProperty("source", out urlElement)) streamUrl = urlElement.GetString();
                        }
                        else if (root.TryGetProperty("streamUrl", out var directUrlElement)) streamUrl = directUrlElement.GetString();
                        else if (root.TryGetProperty("stream_url", out directUrlElement)) streamUrl = directUrlElement.GetString();
                        else if (root.TryGetProperty("url", out directUrlElement)) streamUrl = directUrlElement.GetString();
                        
                        if (!string.IsNullOrEmpty(streamUrl))
                        {
                            var streamResponse = new StreamResponse
                            {
                                Success = true,
                                Data = new StreamData { StreamUrl = streamUrl }
                            };
                            CacheResponse(gmid, streamResponse);
                            return streamResponse;
                        }
                    }
                    catch { }
                }
            }
            else if ((int)response.StatusCode == 429)
            {
                lock (_cacheLock)
                {
                    if (_cache.TryGetValue(gmid, out var cached))
                    {
                        return cached.Response;
                    }
                }

                return new StreamResponse
                {
                    Success = false,
                    Message = "Rate limit reached. Please try again later."
                };
            }

            return new StreamResponse
            {
                Success = false,
                Message = $"Failed to fetch stream: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new StreamResponse
            {
                Success = false,
                Message = $"Error fetching stream: {ex.Message}"
            };
        }
    }

    private void CacheResponse(long gmid, StreamResponse response)
    {
        if (!response.Success)
        {
            return;
        }

        lock (_cacheLock)
        {
            _cache[gmid] = (DateTime.UtcNow.Add(CacheDuration), response);
        }
    }
}
