using Microsoft.Extensions.Options;

namespace wl.chat.Data;

public class ChatService
{
    private readonly HttpClient _httpClient;
    private readonly IDbConnection _db;
    private readonly string _baseUrl;

    public ChatService(HttpClient httpClient, IDbConnection db, IOptions<LMStudioOptions> options)
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

    public async Task SaveChatAsync(string userId, string prompt, string response, string model)
    {
        var sql = @"INSERT INTO ChatHistory (UserId, Prompt, Response, Model, CreatedAt)
                        VALUES (@UserId, @Prompt, @Response, @Model, @CreatedAt)";
        await _db.ExecuteAsync(sql, new
        {
            UserId = userId,
            Prompt = prompt,
            Response = response,
            Model = model,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<List<ChatThread>> GetThreadsForUserAsync(string userId)
    {
        var sql = "SELECT * FROM ChatThread WHERE UserId = @UserId ORDER BY CreatedAt DESC";
        var threads = await _db.QueryAsync<ChatThread>(sql, new { UserId = userId });
        return threads.ToList();
    }

    public async Task<int> CreateThreadAsync(string userId, string title)
    {
        var sql = @"INSERT INTO ChatThread (UserId, Title, CreatedAt)
                VALUES (@UserId, @Title, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";
        return await _db.ExecuteScalarAsync<int>(sql, new
        {
            UserId = userId,
            Title = title,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<List<ChatHistory>> GetMessagesForThreadAsync(int threadId)
    {
        var sql = "SELECT * FROM ChatHistory WHERE ThreadId = @ThreadId ORDER BY CreatedAt";
        var messages = await _db.QueryAsync<ChatHistory>(sql, new { ThreadId = threadId });
        return messages.ToList();
    }

    public async Task SaveChatAsync(int threadId, string userId, string prompt, string response, string model)
    {
        var sql = @"INSERT INTO ChatHistory (ThreadId, UserId, Prompt, Response, Model, CreatedAt)
                VALUES (@ThreadId, @UserId, @Prompt, @Response, @Model, @CreatedAt)";
        await _db.ExecuteAsync(sql, new
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
        // Delete messages first (if you have ON DELETE CASCADE, this is optional)
        var deleteMessagesSql = "DELETE FROM ChatHistory WHERE ThreadId = @ThreadId";
        await _db.ExecuteAsync(deleteMessagesSql, new { ThreadId = threadId });

        // Then delete the thread
        var deleteThreadSql = "DELETE FROM ChatThread WHERE Id = @ThreadId";
        await _db.ExecuteAsync(deleteThreadSql, new { ThreadId = threadId });
    }
}