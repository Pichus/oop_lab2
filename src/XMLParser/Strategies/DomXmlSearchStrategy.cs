using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using XMLParser.Models;

namespace XMLParser.Strategies
{
    public class DomXmlSearchStrategy : IXmlSearchStrategy
    {
        public string Name => "DOM (XmlDocument)";

        public IEnumerable<SearchResult> Search(
            string xmlPath,
            string nodeName,
            string? attributeName,
            string? attributeValue,
            string? keyword)
        {
            var doc = new XmlDocument();
            doc.Load(xmlPath);

            var nodes = doc.GetElementsByTagName(nodeName);
            if (nodes == null) yield break;

            foreach (XmlNode node in nodes)
            {
                // Collect child ELEMENTS as field/value pairs
                var childElements = node.ChildNodes
                    .OfType<XmlElement>()
                    .ToList();

                var attrSummary = new List<string>();
                string? foundAttrValue = null;

                foreach (var el in childElements)
                {
                    var key = el.Name;
                    var val = el.InnerText ?? string.Empty;

                    attrSummary.Add($"{key}={val}");

                    if (attributeName != null && string.Equals(key, attributeName, StringComparison.OrdinalIgnoreCase))
                        foundAttrValue = val;
                }

                bool attrMatches =
                    attributeName is null ||
                    attributeValue is null ||
                    foundAttrValue == attributeValue;

                var innerText = node.InnerText ?? string.Empty;

                bool keywordMatches =
                    string.IsNullOrWhiteSpace(keyword) ||
                    innerText.Contains(keyword, StringComparison.OrdinalIgnoreCase);

                if (attrMatches && keywordMatches)
                {
                    yield return new SearchResult
                    {
                        NodeName = node.Name,
                        AttributeSummary = string.Join(", ", attrSummary),
                        TextContent = innerText
                    };
                }
            }
        }

        public IEnumerable<string> GetAttributeNames(string xmlPath, string nodeName)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var doc = new XmlDocument();
            doc.Load(xmlPath);

            var nodes = doc.GetElementsByTagName(nodeName);
            if (nodes == null) return names;

            foreach (XmlNode node in nodes)
            {
                foreach (XmlElement el in node.ChildNodes.OfType<XmlElement>())
                    names.Add(el.Name);
            }

            return names;
        }

        public IDictionary<string, HashSet<string>> GetAttributeValues(string xmlPath, string nodeName)
        {
            var dict = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            var doc = new XmlDocument();
            doc.Load(xmlPath);

            var nodes = doc.GetElementsByTagName(nodeName);

            foreach (XmlNode node in nodes)
            {
                foreach (XmlElement el in node.ChildNodes.OfType<XmlElement>())
                {
                    var key = el.Name;
                    var val = el.InnerText ?? string.Empty;

                    if (!dict.TryGetValue(key, out var set))
                    {
                        set = new HashSet<string>();
                        dict[key] = set;
                    }

                    set.Add(val);
                }
            }

            return dict;
        }
    }
}
