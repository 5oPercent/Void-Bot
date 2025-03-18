using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Settings;
using Void.Bot.Message.Commands.Identity;

namespace Void.Bot.Core.Services.Identity;

public interface IApiKeyService : IScope
{
    Task<CreateApiKeyResponse> CreateApiKeyAsync(CreateApiKeyCommand command, CancellationToken cancellationToken);
    
    Task<DeleteApiKeyResponse> DeleteApiKeyAsync(DeleteApiKeyCommand command, CancellationToken cancellationToken);
    
    string Encrypt(string source);
    
    string Decrypt(string source);
}

public class ApiKeyService : IApiKeyService
{
    private readonly ICurrentUser _currentUser;
    private readonly IRepository _repository;
    
    public ApiKeyService(ICurrentUser currentUser, IRepository repository)
    {
        _currentUser = currentUser;
        _repository = repository;
    }

    public async Task<CreateApiKeyResponse> CreateApiKeyAsync(CreateApiKeyCommand command, CancellationToken cancellationToken)
    {
        var existingLicense = await _repository.QueryNoTracking<SystemLicenses>()
            .FirstOrDefaultAsync(x => x.ApiKey == command.ApiKey, cancellationToken).ConfigureAwait(false);
        
        if (existingLicense is not null)
        {
            return new CreateApiKeyResponse
            {
                Code = HttpStatusCode.InternalServerError,
                Message = "已存在相同的ApiKey"
            };
        }
        
        var apikey = Encrypt(command.ApiKey);
        
        var license = new SystemLicenses
        {
            ApiKey = command.ApiKey,
            UserName = _currentUser.UserName
        };
        
        await _repository.InsertAsync(license, cancellationToken).ConfigureAwait(false);

        return new CreateApiKeyResponse
        {
            Data = apikey
        };
    }

    public async Task<DeleteApiKeyResponse> DeleteApiKeyAsync(DeleteApiKeyCommand command, CancellationToken cancellationToken)
    {
        var existingLicense = await _repository.QueryNoTracking<SystemLicenses>()
            .WhereIF(command.UserId.HasValue, x => x.CreatedBy == command.UserId)
            .FirstOrDefaultAsync(x => x.ApiKey == command.ApiKey, cancellationToken).ConfigureAwait(false);

        if (existingLicense is not null)
        {
            await _repository.DeleteAsync(existingLicense, cancellationToken).ConfigureAwait(false);
        }
        
        return new DeleteApiKeyResponse();
    }

    private byte[] GetKey()
    {
        var data = Encoding.GetEncoding("utf-8").GetBytes(AppSetting.SecretKey);

        return MD5.HashData(data);
    }
    
    public string Encrypt(string source)
    {
        using var aesProvider = Aes.Create();

        aesProvider.Key = GetKey();

        aesProvider.Mode = CipherMode.ECB;

        aesProvider.BlockSize = 128;

        aesProvider.Padding = PaddingMode.PKCS7;

        using var cryptoTransform = aesProvider.CreateEncryptor();

        var inputBuffers = Encoding.UTF8.GetBytes(source);

        var results = cryptoTransform.TransformFinalBlock(inputBuffers, 0, inputBuffers.Length);

        aesProvider.Clear();

        aesProvider.Dispose();

        return Convert.ToBase64String(results, 0, results.Length);
    }

    public string Decrypt(string source)
    {
        using var aesProvider = Aes.Create();

        aesProvider.Key = GetKey();

        aesProvider.Mode = CipherMode.ECB;

        aesProvider.BlockSize = 128;

        aesProvider.Padding = PaddingMode.PKCS7;

        using var cryptoTransform = aesProvider.CreateDecryptor();

        var inputBuffers = Convert.FromBase64String(source);

        var results = cryptoTransform.TransformFinalBlock(inputBuffers, 0, inputBuffers.Length);

        aesProvider.Clear();

        return Encoding.UTF8.GetString(results);
    }
}