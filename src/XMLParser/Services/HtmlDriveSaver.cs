using System;
using System.IO;
using System.Net;

namespace XMLParser.Services;

public class HtmlDriveSaver : FilteredDataSaver
{
    private readonly XslTransformService _transformer;

    public HtmlDriveSaver(string credentialsJsonPath)
        : base(credentialsJsonPath)
    {
        _transformer = new XslTransformService();
    }

    public override string SaveToLocal(string xmlFragment)
    {
        var tempXml = Path.Combine(Path.GetTempPath(), $"fragment_{DateTime.Now:yyyyMMdd_HHmmss}.xml");
        File.WriteAllText(tempXml, xmlFragment ?? string.Empty);

        var html = $"<html><body><pre>{WebUtility.HtmlEncode(xmlFragment)}</pre></body></html>";

        var fileName = $"filtered_{DateTime.Now:yyyyMMdd_HHmmss}.html";
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
        File.WriteAllText(path, html);
        return path;
    }
}