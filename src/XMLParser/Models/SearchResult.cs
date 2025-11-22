namespace XMLParser.Models;

public class SearchResult
{
    public string NodeName { get; init; } = string.Empty;
    public string? AttributeSummary { get; init; }
    public string? TextContent { get; init; }

    public override string ToString()
    {
        return $"{NodeName} | {AttributeSummary} | {TextContent}";
    }
}