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
        string? attributeName,
        string? attributeValue,
        string? keyword)
    {
        var doc = XDocument.Load(xmlPath);

        var query = doc.Root?
            .Elements("book") ?? [];

        foreach (var book in query)
        {
            var attrs = book.Attributes().ToList();

            string? foundAttrValue = null;
            if (attributeName is not null)
            {
                var attr = attrs.FirstOrDefault(a => a.Name.LocalName == attributeName);
                foundAttrValue = attr?.Value;
            }

            var attrMatches = attributeName is null
                              || attributeValue is null
                              || foundAttrValue == attributeValue;

            var innerText = string.Join(" ", book.Elements().Select(e => e.Value));
            var keywordMatches = string.IsNullOrWhiteSpace(keyword)
                                 || innerText.Contains(keyword,
                                     StringComparison.OrdinalIgnoreCase);

            if (attrMatches && keywordMatches)
                yield return new SearchResult
                {
                    NodeName = book.Name.LocalName,
                    AttributeSummary = string.Join(", ", attrs.Select(a => $"{a.Name}={a.Value}")),
                    TextContent = innerText
                };
        }
    }

    public IEnumerable<string> GetAttributeNames(string xmlPath)
    {
        var doc = XDocument.Load(xmlPath);

        return doc.Root?
                   .Elements("book")
                   .SelectMany(b => b.Attributes())
                   .Select(a => a.Name.LocalName)
                   .Distinct()
               ?? [];
    }

    public IDictionary<string, HashSet<string>> GetAttributeValues(string xmlPath)
    {
        var doc = XDocument.Load(xmlPath);
        var dict = new Dictionary<string, HashSet<string>>();

        var books = doc.Root?.Elements("book") ?? [];
        foreach (var b in books)
        foreach (var attr in b.Attributes())
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