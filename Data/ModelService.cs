using Microsoft.Extensions.Options;

namespace wl.chat.Data;

public class ModelService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ModelService(HttpClient httpClient, IOptions<LMStudioOptions> options)
    {
        _httpClient = httpClient;
        _baseUrl = options.Value.BaseUrl.TrimEnd('/');
    }

    public async Task<List<ModelInfo>> GetAvailableModelsAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/v1/models");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Parse the JSON to match your API's structure
        var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");
        var models = new List<ModelInfo>();
        foreach (var item in data.EnumerateArray())
        {
            models.Add(new ModelInfo
            {
                Id = item.GetProperty("id").GetString()
            });
        }
        return models;
    }

    private class ModelListResponse
    {
        public List<ModelInfo> Data { get; set; }
    }
}