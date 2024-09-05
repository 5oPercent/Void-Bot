using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Autofac;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.HttpClient;

namespace Void.Bot.Core.Services.Http;

public interface IVoidBotHttpClientFactory : IScope
{
    HttpClient CreateClient(TokenModel? tokenModel = null, Dictionary<string, string>? headers = null, TimeSpan? timeout = null, bool beginScope = false);
    
    Task<VoidBotResponse<T>> GetAsync<T>(string url, CancellationToken cancellationToken, TimeSpan? timeout = null, TokenModel? tokenModel = null, Dictionary<string, string>? headers = null, bool beginScope = false);
    
    Task<VoidBotResponse<T>> PostAsync<T>(string url, HttpContent content, CancellationToken cancellationToken, TimeSpan? timeout = null, TokenModel? tokenModel = null, Dictionary<string, string>? headers = null, bool beginScope = false);
    
    Task<VoidBotResponse<T>> PostAsync<T>(string url, object content, CancellationToken cancellationToken, TimeSpan? timeout = null, TokenModel? tokenModel = null, Dictionary<string, string>? headers = null, bool beginScope = false);
}

public class VoidBotHttpClientFactory(ILifetimeScope scope) : IVoidBotHttpClientFactory
{
    public HttpClient CreateClient(TokenModel? tokenModel = null, Dictionary<string, string>? headers = null, TimeSpan? timeout = null, bool beginScope = false)
    {
        var scope1 = beginScope ? scope.BeginLifetimeScope() : scope;

        var client = scope1.TryResolve(out IHttpClientFactory? httpClientFactory)
            ? httpClientFactory.CreateClient()
            : new HttpClient();
        
        if (timeout != null)
            client.Timeout = timeout.Value;

        if (tokenModel != null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenModel.Scheme, tokenModel.Parameter);

        if (headers == null) return client;
        
        foreach (var header in headers)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        return client;
    }
    
    public async Task<VoidBotResponse<T>> GetAsync<T>(string url, CancellationToken cancellationToken, TimeSpan? timeout = null, TokenModel? token = null, Dictionary<string, string>? headers = null, bool beginScope = false)
    {
        return await SafeProcessAsync(async () =>
        {
            var response = await CreateClient(token, headers, timeout, beginScope)
                .GetAsync(url, cancellationToken).ConfigureAwait(false);
            
            var data = await ReadResponseContentAsAsync<T>(response, cancellationToken).ConfigureAwait(false);

            return new VoidBotResponse<T>
            {
                Code = response.StatusCode,
                Data = data
            };
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VoidBotResponse<T>> PostAsync<T>(string url, HttpContent content, CancellationToken cancellationToken, TimeSpan? timeout = null, TokenModel? token = null, Dictionary<string, string>? headers = null, bool beginScope = false)
    {
        return await SafeProcessAsync(async () =>
        {
            var response = await CreateClient(token, headers, timeout, beginScope)
                .PostAsync(url, content, cancellationToken).ConfigureAwait(false);
            
            var data = await ReadResponseContentAsAsync<T>(response, cancellationToken).ConfigureAwait(false);
            
            return new VoidBotResponse<T>
            {
                Code = response.StatusCode,
                Data = data
            };
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VoidBotResponse<T>> PostAsync<T>(string url, object content, CancellationToken cancellationToken, TimeSpan? timeout = null, TokenModel? token = null, Dictionary<string, string>? headers = null, bool beginScope = false)
    {
        return await SafeProcessAsync(async () =>
        {
            var response = await CreateClient(token, headers, timeout, beginScope)
                .PostAsJsonAsync(url, content, cancellationToken).ConfigureAwait(false);
            
            var data = await ReadResponseContentAsAsync<T>(response, cancellationToken).ConfigureAwait(false);

            return new VoidBotResponse<T>
            {
                Code = response.StatusCode,
                Data = data
            };
        }, cancellationToken).ConfigureAwait(false);
    }
    
    private static async Task<VoidBotResponse<T>> SafeProcessAsync<T>(Func<Task<VoidBotResponse<T>>> func, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            return await func().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new VoidBotResponse<T>
            {
                Code = HttpStatusCode.BadRequest,
                Message = ex.Message
            };
        }
    }
    
    private static async Task<T> ReadResponseContentAsAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (typeof(T) == typeof(string))
            return (T)(object) await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        
        if (typeof(T) == typeof(byte))
            return (T)(object) await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        
        if (typeof(T) == typeof(Stream))
            return (T)(object) await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException();
    }
}