using System;
using System.Collections.Generic;
using System.Xml;
using XMLParser.Models;

namespace XMLParser.Strategies;

public class SaxXmlSearchStrategy : IXmlSearchStrategy
{
    public string Name => "SAX (XmlReader)";

    public IEnumerable<SearchResult> Search(
        string xmlPath,
        string? attributeName,
        string? attributeValue,
        string? keyword)
    {
        using var reader = XmlReader.Create(xmlPath, new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true
        });

        while (reader.Read())
            if (reader.NodeType == XmlNodeType.Element && reader.Name == "book")
            {
                var attrSummary = new List<string>();
                string? foundAttrValue = null;

                if (reader.HasAttributes)
                {
                    while (reader.MoveToNextAttribute())
                    {
                        attrSummary.Add($"{reader.Name}={reader.Value}");

                        if (!string.IsNullOrEmpty(attributeName) &&
                            reader.Name == attributeName)
                            foundAttrValue = reader.Value;
                    }

                    reader.MoveToElement();
                }

                var attrMatches = attributeName is null
                                  || attributeValue is null
                                  || foundAttrValue == attributeValue;

                var innerText = reader.ReadInnerXml();
                var keywordMatches = string.IsNullOrWhiteSpace(keyword)
                                     || innerText.Contains(keyword,
                                         StringComparison.OrdinalIgnoreCase);

                if (attrMatches && keywordMatches)
                    yield return new SearchResult
                    {
                        NodeName = "book",
                        AttributeSummary = string.Join(", ", attrSummary),
                        TextContent = innerText
                    };
            }
    }

    public IEnumerable<string> GetAttributeNames(string xmlPath)
    {
        var names = new HashSet<string>();

        using var reader = XmlReader.Create(xmlPath);
        while (reader.Read())
            if (reader.NodeType == XmlNodeType.Element && reader.Name == "book" && reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute()) names.Add(reader.Name);
                reader.MoveToElement();
            }

        return names;
    }

    public IDictionary<string, HashSet<string>> GetAttributeValues(string xmlPath)
    {
        var dict = new Dictionary<string, HashSet<string>>();

        using var reader = XmlReader.Create(xmlPath);
        while (reader.Read())
            if (reader.NodeType == XmlNodeType.Element && reader.Name == "book" && reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    if (!dict.TryGetValue(reader.Name, out var set))
                    {
                        set = new HashSet<string>();
                        dict[reader.Name] = set;
                    }

                    set.Add(reader.Value);
                }

                reader.MoveToElement();
            }

        return dict;
    }
}