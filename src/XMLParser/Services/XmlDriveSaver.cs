using System;
using System.IO;

namespace XMLParser.Services;

public class XmlDriveSaver : FilteredDataSaver
{
    public XmlDriveSaver(string credentialsJsonPath)
        : base(credentialsJsonPath)
    {
    }

    public override string SaveToLocal(string xmlFragment)
    {
        var fileName = $"filtered_{DateTime.Now:yyyyMMdd_HHmmss}.xml";
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
        File.WriteAllText(path, xmlFragment ?? string.Empty);
        return path;
    }
}