namespace wl.chat.Data;

public class ChatHistory
{
    public int Id { get; set; }
    public int ThreadId { get; set; }
    public string UserId { get; set; }
    public string Prompt { get; set; }
    public string Response { get; set; }
    public string Model { get; set; }
    public DateTime CreatedAt { get; set; }
}