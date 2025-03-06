using NETCORE.Utils.ConfigOptions;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;

public interface ICloudStorageService
{
    Task<string> GetSignedUrlAsync(string fileNameToRead, int timeOutInMinutes = 30);
    Task<string> UploadFileAsync(IFormFile fileToUpload, string fileNameToSave);
    Task DeleteFileAsync(string fileNameToDelete);
}

public class CloudStorageService : ICloudStorageService
{
    private readonly GoogleCloudStorageConfigOptions _options;
    private readonly ILogger<CloudStorageService> _logger;
    private readonly GoogleCredential _googleCredential;

    public CloudStorageService(IOptions<GoogleCloudStorageConfigOptions> options, ILogger<CloudStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;

        try
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == Environments.Production)
            {
                // Store the json file in Secrets.
                _googleCredential = GoogleCredential.FromJson(_options.AuthFile);
            }
            else
            {
                _googleCredential = GoogleCredential.FromFile(_options.AuthFile);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"{ex.Message}");
            throw;
        }
    }
    public async Task DeleteFileAsync(string fileNameToDelete)
    {
        try
        {
            using (var storageClient = StorageClient.Create(_googleCredential))
            {
                await storageClient.DeleteObjectAsync(_options.BucketName, fileNameToDelete);
            }
            _logger.LogInformation($"File {fileNameToDelete} deleted");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error occured while deleting file {fileNameToDelete}: {ex.Message}");
            throw;
        }
    }

    public async Task<string> GetSignedUrlAsync(string fileNameToRead, int timeOutInMinutes = 30)
    {
        try
        {
            var sac = _googleCredential.UnderlyingCredential as ServiceAccountCredential;
            var urlSigner = UrlSigner.FromCredential(sac); // Updated method
            var signedUrl = await urlSigner.SignAsync(_options.BucketName, fileNameToRead, TimeSpan.FromMinutes(timeOutInMinutes));
            _logger.LogInformation($"Signed URL obtained for file {fileNameToRead}");
            return signedUrl.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error occurred while obtaining signed URL for file {fileNameToRead}: {ex.Message}");
            throw;
        }
    }

    public async Task<string> UploadFileAsync(IFormFile fileToUpload, string fileNameToSave)
    {
        try
        {
            _logger.LogInformation($"Uploading: file {fileNameToSave} to storage {_options.BucketName}");
            using (var memoryStream = new MemoryStream())
            {
                await fileToUpload.CopyToAsync(memoryStream);
                // Create Storage Client from Google Credential
                using (var storageClient = StorageClient.Create(_googleCredential))
                {
                    // upload file stream
                    var uploadedFile = await storageClient.UploadObjectAsync(_options.BucketName, fileNameToSave, fileToUpload.ContentType, memoryStream);
                    _logger.LogInformation($"Uploaded: file {fileNameToSave} to storage {_options.BucketName}");
                    return uploadedFile.MediaLink;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while uploading file {fileNameToSave}: {ex.Message}");
            throw;
        }
    }
}