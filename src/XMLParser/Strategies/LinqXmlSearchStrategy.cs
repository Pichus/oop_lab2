using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using XMLParser.Models;

namespace XMLParser.Strategies;

public class LinqXmlSearchStrategy : IXmlSearchStrategy
{
    public string Name => "LINQ to XML (XDocument)";

    public IEnumerable<SearchResult> Search(
        string xmlPath,
        string nodeName,
        string? attributeName,
        string? attributeValue,
        string? keyword)
    {
        var doc = XDocument.Load(xmlPath);

        var nodes = doc.Descendants(nodeName);

        foreach (var node in nodes)
        {
            var attrs = node.Elements().ToList();

            var foundAttrValue = attributeName is not null
                ? attrs.FirstOrDefault(a => a.Name.LocalName == attributeName)?.Value
                : null;

            var attrMatches =
                attributeName is null ||
                attributeValue is null ||
                foundAttrValue == attributeValue;

            var innerText = string.Join(" ", node.Elements().Select(e => e.Value));

            var keywordMatches =
                string.IsNullOrWhiteSpace(keyword) ||
                innerText.Contains(keyword, StringComparison.OrdinalIgnoreCase);

            if (attrMatches && keywordMatches)
                yield return new SearchResult
                {
                    NodeName = node.Name.LocalName,
                    AttributeSummary = string.Join(", ", attrs.Select(a => $"{a.Name}={a.Value}")),
                    TextContent = innerText
                };
        }
    }

    public IEnumerable<string> GetAttributeNames(string xmlPath, string nodeName)
    {
        var doc = XDocument.Load(xmlPath);

        return doc.Descendants(nodeName)
            .SelectMany(n => n.Elements())
            .Select(a => a.Name.LocalName)
            .Distinct();
    }

    public IDictionary<string, HashSet<string>> GetAttributeValues(string xmlPath, string nodeName)
    {
        var doc = XDocument.Load(xmlPath);

        var dict = new Dictionary<string, HashSet<string>>();

        foreach (var attr in doc.Descendants(nodeName).SelectMany(n => n.Elements()))
        {
            if (!dict.TryGetValue(attr.Name.LocalName, out var set))
            {
                set = new HashSet<string>();
                dict[attr.Name.LocalName] = set;
            }

            set.Add(attr.Value);
        }

        return dict;
    }
}