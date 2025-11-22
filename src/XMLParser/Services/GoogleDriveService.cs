using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;

namespace XMLParser.Services;

public class GoogleDriveService : IDisposable
{
    private readonly DriveService _driveService;

    public GoogleDriveService(string credentialsJsonPath, string applicationName = "XMLParserApp")
    {
        if (string.IsNullOrWhiteSpace(credentialsJsonPath))
            throw new ArgumentNullException(nameof(credentialsJsonPath));

        GoogleCredential credential;
        using (var stream = new FileStream(credentialsJsonPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped(DriveService.ScopeConstants.DriveFile, DriveService.ScopeConstants.Drive);
        }

        _driveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = applicationName
        });
    }

    public void Dispose()
    {
        _driveService?.Dispose();
    }

    public async Task<string> UploadFileAsync(string localFilePath, string? folderId = null,
        CancellationToken ct = default)
    {
        if (!File.Exists(localFilePath)) throw new FileNotFoundException(localFilePath);

        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = Path.GetFileName(localFilePath)
        };

        if (!string.IsNullOrWhiteSpace(folderId))
            fileMetadata.Parents = new[] { folderId };

        using var fs = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);

        var request = _driveService.Files.Create(fileMetadata, fs, GetMimeType(localFilePath));
        request.Fields = "id, webViewLink, webContentLink";

        var progress = await request.UploadAsync(ct).ConfigureAwait(false);
        if (progress.Status != UploadStatus.Completed)
            throw new Exception($"Upload failed: {progress.Exception?.Message ?? progress.Status.ToString()}");

        var file = request.ResponseBody ?? throw new Exception("Upload returned null response");
        return file.Id!;
    }

    private static string GetMimeType(string path)
    {
        var ext = Path.GetExtension(path)?.ToLowerInvariant();
        return ext switch
        {
            ".html" => "text/html",
            ".htm" => "text/html",
            ".xml" => "application/xml",
            ".xsl" => "application/xml",
            ".txt" => "text/plain",
            ".json" => "application/json",
            _ => "application/octet-stream"
        };
    }
}