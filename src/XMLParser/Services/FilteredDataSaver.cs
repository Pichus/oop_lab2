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

    /// <summary>
    ///     Save filtered fragment to local file and return local path.
    /// </summary>
    public abstract string SaveToLocal(string xmlFragment);

    /// <summary>
    ///     Save and upload to Google Drive. Returns uploaded file id.
    /// </summary>
    public virtual async Task<string> SaveToDriveAsync(string xmlFragment)
    {
        var local = SaveToLocal(xmlFragment);
        AppLogger.Instance.LogEvent(AppLogger.EventType.Saving, $"Збережено локально у {local}");

        using var g = new GoogleDriveService(CredentialsJsonPath);
        var id = await g.UploadFileAsync(local);
        AppLogger.Instance.LogEvent(AppLogger.EventType.Saving, $"Завантажено на Drive. FileId={id}");
        return id;
    }

    public static FilteredDataSaver CreateSaver(string extension, string credentialsJsonPath)
    {
        return extension.ToLowerInvariant() switch
        {
            ".xml" => new XmlDriveSaver(credentialsJsonPath),
            ".html" => new HtmlDriveSaver(credentialsJsonPath),
            _ => throw new NotSupportedException($"Unknown format {extension}")
        };
    }
}