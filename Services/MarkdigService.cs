using Markdig;

namespace wl.chat.Services;

public class MarkdigService : IMarkdownService
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdigService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseEmojiAndSmiley()
            .UseSoftlineBreakAsHardlineBreak()
            .Build();
    }

    public string Parse(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return string.Empty;
        }

        return Markdown.ToHtml(markdown, _pipeline);
    }

}