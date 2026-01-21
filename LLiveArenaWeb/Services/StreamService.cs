using LLiveArenaWeb.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LLiveArenaWeb.Services;

public class StreamService : IStreamService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string RapidApiHost = "all-sport-live-stream.p.rapidapi.com";
    private const string RapidApiKey = "49eb2c2a31mshb8ed05c07896df9p120e09jsn67fc0221f12d";
    private const string BaseUrl = "https://all-sport-live-stream.p.rapidapi.com/api/d/stream_source";

    public StreamService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<StreamResponse?> GetStreamSourceAsync(long gmid)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", RapidApiHost);
            httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", RapidApiKey);
            
            var url = $"{BaseUrl}?gmid={gmid}";
            var response = await httpClient.GetAsync(url);

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
                            return new StreamResponse
                            {
                                Success = true,
                                Data = new StreamData { StreamUrl = streamUrl }
                            };
                        }
                    }
                    catch { }
                }
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
}
