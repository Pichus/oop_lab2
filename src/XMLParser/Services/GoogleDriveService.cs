using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;

namespace XMLParser.Services;

public class GoogleDriveService : IDisposable
{
    private DriveService _driveService;
    private readonly string _credentialsJsonPath;
    private readonly string _applicationName;

    public GoogleDriveService(string credentialsJsonPath, string applicationName = "XMLParserApp")
    {
        if (string.IsNullOrWhiteSpace(credentialsJsonPath))
            throw new ArgumentNullException(nameof(credentialsJsonPath));

        _credentialsJsonPath = credentialsJsonPath;

        _applicationName = applicationName;
    }

    public async Task Initialize()
    {
        var credential = await AuthorizeAsync(_credentialsJsonPath, _applicationName);

        _driveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _applicationName
        });
    }

    private static async Task<UserCredential> AuthorizeAsync(string credentialsJsonPath, string appName)
    {
        using var stream = new FileStream(credentialsJsonPath, FileMode.Open, FileAccess.Read);
        string credPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), 
            ".credentials", appName);

        return await GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.FromStream(stream).Secrets,
            new[] { DriveService.Scope.DriveFile }, // limited access
            "user",
            CancellationToken.None,
            new FileDataStore(credPath, true)
        );
    }

    public async Task<string> UploadFileAsync(string localFilePath, string? folderId = null,
        CancellationToken ct = default)
    {
        if (!File.Exists(localFilePath))
            throw new FileNotFoundException(localFilePath);

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

    public void Dispose()
    {
        _driveService?.Dispose();
    }
}