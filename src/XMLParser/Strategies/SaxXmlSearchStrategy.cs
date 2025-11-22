using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using XMLParser.Models;

namespace XMLParser.Strategies;

public class SaxXmlSearchStrategy : IXmlSearchStrategy
{
    public string Name => "SAX (XmlReader)";

    public IEnumerable<SearchResult> Search(
        string xmlPath,
        string nodeName,
        string? attributeName,
        string? attributeValue,
        string? keyword)
    {
        var settings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            DtdProcessing = DtdProcessing.Ignore
        };

        using var reader = XmlReader.Create(xmlPath, settings);

        while (reader.ReadToFollowing(nodeName))
        {
            using var subtree = reader.ReadSubtree();
            subtree.MoveToContent();
            XElement el;
            try
            {
                el = XElement.Load(subtree);
            }
            catch
            {
                continue;
            }

            var attrs = el.Elements().ToList();

            string? foundAttrValue = null;
            if (!string.IsNullOrEmpty(attributeName))
                foundAttrValue = attrs.FirstOrDefault(a => a.Name.LocalName == attributeName)?.Value;

            var attrMatches = attributeName is null
                              || attributeValue is null
                              || foundAttrValue == attributeValue;

            var innerText = string.Join(" ",
                el.Elements().Select(e => e.Value).Where(v => !string.IsNullOrWhiteSpace(v)));

            var keywordMatches = string.IsNullOrWhiteSpace(keyword)
                                 || innerText.Contains(keyword, StringComparison.OrdinalIgnoreCase);

            if (attrMatches && keywordMatches)
                yield return new SearchResult
                {
                    NodeName = el.Name.LocalName,
                    AttributeSummary = string.Join(", ", attrs.Select(a => $"{a.Name.LocalName}={a.Value}")),
                    TextContent = innerText
                };
        }
    }

    public IEnumerable<string> GetAttributeNames(string xmlPath, string nodeName)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var settings = new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true };

        using var reader = XmlReader.Create(xmlPath, settings);

        while (reader.ReadToFollowing(nodeName))
        {
            using var subtree = reader.ReadSubtree();
            subtree.MoveToContent();
            XElement el;
            try
            {
                el = XElement.Load(subtree);
            }
            catch
            {
                continue;
            }

            foreach (var a in el.Elements())
                names.Add(a.Name.LocalName);
        }

        return names;
    }

    public IDictionary<string, HashSet<string>> GetAttributeValues(string xmlPath, string nodeName)
    {
        var dict = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var settings = new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true };

        using var reader = XmlReader.Create(xmlPath, settings);

        while (reader.ReadToFollowing(nodeName))
        {
            using var subtree = reader.ReadSubtree();
            subtree.MoveToContent();
            XElement el;
            try
            {
                el = XElement.Load(subtree);
            }
            catch
            {
                continue;
            }

            foreach (var a in el.Elements())
            {
                if (!dict.TryGetValue(a.Name.LocalName, out var set))
                {
                    set = new HashSet<string>();
                    dict[a.Name.LocalName] = set;
                }

                set.Add(a.Value);
            }
        }

        return dict;
    }
}