using System;
using System.Collections.Generic;
using XMLParser.Models;

namespace XMLParser.ViewModels;

public class SearchResultViewModel
{
    public SearchResultViewModel(SearchResult result)
    {
        NodeName = result.NodeName;
        TextContent = result.TextContent;

        foreach (var pair in result.AttributeSummary.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2)
                Attributes.Add(new KeyValuePair<string, string>(parts[0].Trim(), parts[1].Trim()));
        }
    }

    public string NodeName { get; set; } = string.Empty;
    public List<KeyValuePair<string, string>> Attributes { get; set; } = new();
    public string TextContent { get; set; } = string.Empty;
}