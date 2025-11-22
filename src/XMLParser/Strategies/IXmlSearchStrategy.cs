using System.Collections.Generic;
using XMLParser.Models;

namespace XMLParser.Strategies;

public interface IXmlSearchStrategy
{
    string Name { get; }

    IEnumerable<SearchResult> Search(
        string xmlPath,
        string nodeName,
        string? attributeName,
        string? attributeValue,
        string? keyword
    );

    IEnumerable<string> GetAttributeNames(string xmlPath, string nodeName);
    IDictionary<string, HashSet<string>> GetAttributeValues(string xmlPath, string nodeName);
}