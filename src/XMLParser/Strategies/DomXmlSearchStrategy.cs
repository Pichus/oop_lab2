using System;
using System.Collections.Generic;
using System.Xml;
using XMLParser.Models;

namespace XMLParser.Strategies;

public class DomXmlSearchStrategy : IXmlSearchStrategy
{
    public string Name => "DOM (XmlDocument)";

    public IEnumerable<SearchResult> Search(
        string xmlPath,
        string? attributeName,
        string? attributeValue,
        string? keyword)
    {
        var doc = new XmlDocument();
        doc.Load(xmlPath);

        var nodes = doc.SelectNodes("//book");
        if (nodes is null) yield break;

        foreach (XmlNode node in nodes)
        {
            var attrSummary = new List<string>();
            string? foundAttrValue = null;

            if (node.Attributes is not null)
                foreach (XmlAttribute attr in node.Attributes)
                {
                    attrSummary.Add($"{attr.Name}={attr.Value}");
                    if (attributeName is not null && attr.Name == attributeName)
                        foundAttrValue = attr.Value;
                }

            var attrMatches = attributeName is null
                              || attributeValue is null
                              || foundAttrValue == attributeValue;

            var innerText = node.InnerText;
            var keywordMatches = string.IsNullOrWhiteSpace(keyword)
                                 || innerText.Contains(keyword,
                                     StringComparison.OrdinalIgnoreCase);

            if (attrMatches && keywordMatches)
                yield return new SearchResult
                {
                    NodeName = node.Name,
                    AttributeSummary = string.Join(", ", attrSummary),
                    TextContent = innerText
                };
        }
    }

    public IEnumerable<string> GetAttributeNames(string xmlPath)
    {
        var names = new HashSet<string>();
        var doc = new XmlDocument();
        doc.Load(xmlPath);

        var nodes = doc.SelectNodes("//book");
        if (nodes is null) return names;

        foreach (XmlNode node in nodes)
        {
            if (node.Attributes is null) continue;
            foreach (XmlAttribute attr in node.Attributes) names.Add(attr.Name);
        }

        return names;
    }

    public IDictionary<string, HashSet<string>> GetAttributeValues(string xmlPath)
    {
        var dict = new Dictionary<string, HashSet<string>>();
        var doc = new XmlDocument();
        doc.Load(xmlPath);

        var nodes = doc.SelectNodes("//book");
        if (nodes is null) return dict;

        foreach (XmlNode node in nodes)
        {
            if (node.Attributes is null) continue;
            foreach (XmlAttribute attr in node.Attributes)
            {
                if (!dict.TryGetValue(attr.Name, out var set))
                {
                    set = new HashSet<string>();
                    dict[attr.Name] = set;
                }

                set.Add(attr.Value);
            }
        }

        return dict;
    }
}