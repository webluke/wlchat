using Microsoft.Extensions.Options;

namespace wl.chat.Data;

public class ChatService
{
    private readonly HttpClient _httpClient;
    private readonly ISqlDataService _db;
    private readonly string _baseUrl;

    public ChatService(HttpClient httpClient, ISqlDataService db, IOptions<LMStudioOptions> options)
    {
        _httpClient = httpClient;
        _db = db;
        _baseUrl = options.Value.BaseUrl.TrimEnd('/');
    }

    public async IAsyncEnumerable<string> StreamResponseAsync(int threadId, string userInput, string model)
    {
        var history = await GetMessagesForThreadAsync(threadId);

        // Build the messages array for chat API
        var messages = new List<object>();
        foreach (var msg in history)
        {
            messages.Add(new { role = "user", content = msg.Prompt });
            messages.Add(new { role = "assistant", content = msg.Response });
        }
        messages.Add(new { role = "user", content = userInput });

        var requestBody = new
        {
            model = model,
            messages = messages,
            stream = true
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v1/chat/completions")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            )
        };

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead
        );
        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (!string.IsNullOrWhiteSpace(line))
            {
                if (line.Trim() == "[DONE]")
                    continue;

                string? toYield = null;

                try
                {
                    if (line.StartsWith("data:"))
                        line = line.Substring(5).Trim();

                    using var doc = JsonDocument.Parse(line);
                    if (doc.RootElement.TryGetProperty("choices", out var choices))
                    {
                        var delta = choices[0].GetProperty("delta");
                        if (delta.TryGetProperty("content", out var contentProp))
                        {
                            var text = contentProp.GetString();
                            if (!string.IsNullOrEmpty(text))
                                toYield = text;
                        }
                    }
                    else
                    {
                        toYield = line;
                    }
                }
                catch
                {
                    toYield = line;
                }

                if (!string.IsNullOrEmpty(toYield))
                    yield return toYield;
            }
        }
    }

    public async Task<List<ChatThread>> GetThreadsForUserAsync(string userId)
    {
        var threads = await _db.LoadData<ChatThread, dynamic>("spGetThreadsForUser", new { UserId = userId });
        return threads.ToList();
    }

    public async Task<int> CreateThreadAsync(string userId, string title)
    {
        var result = await _db.LoadData<int, dynamic>("spCreateThread", new
        {
            UserId = userId,
            Title = title,
            CreatedAt = DateTime.UtcNow
        });
        return result.FirstOrDefault();
    }

    public async Task<List<ChatHistory>> GetMessagesForThreadAsync(int threadId)
    {
        var messages = await _db.LoadData<ChatHistory, dynamic>("spGetMessagesForThread", new { ThreadId = threadId });
        return messages.ToList();
    }

    public async Task SaveChatAsync(int threadId, string userId, string prompt, string response, string model)
    {
        await _db.SaveData("spSaveChat", new
        {
            ThreadId = threadId,
            UserId = userId,
            Prompt = prompt,
            Response = response,
            Model = model,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task DeleteThreadAsync(int threadId)
    {
        await _db.SaveData("spDeleteThread", new { ThreadId = threadId });
    }
}