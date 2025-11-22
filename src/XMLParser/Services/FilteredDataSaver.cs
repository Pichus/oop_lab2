using System;
using System.Threading.Tasks;

namespace XMLParser.Services;

public abstract class FilteredDataSaver
{
    protected readonly string CredentialsJsonPath;

    protected FilteredDataSaver(string credentialsJsonPath)
    {
        CredentialsJsonPath = credentialsJsonPath;
    }
    
    public abstract string SaveToLocal(string xmlFragment);
    
    public virtual async Task<string> SaveToDriveAsync(string xmlFragment)
    {
        var local = SaveToLocal(xmlFragment);
        AppLogger.Instance.LogEvent(AppLogger.EventType.Saving, $"Збережено локально у {local}");

        using var g = new GoogleDriveService(CredentialsJsonPath);
        await g.Initialize();
        var id = await g.UploadFileAsync(local);
        AppLogger.Instance.LogEvent(AppLogger.EventType.Saving, $"Завантажено на Drive. FileId={id}");
        return id;
    }

    public static FilteredDataSaver CreateSaver(string extension, string credentialsJsonPath)
    {
        return extension.ToLowerInvariant() switch
        {
            "xml" => new XmlDriveSaver(credentialsJsonPath),
            "html" => new HtmlDriveSaver(credentialsJsonPath),
            _ => throw new NotSupportedException($"Unknown format {extension}")
        };
    }
}