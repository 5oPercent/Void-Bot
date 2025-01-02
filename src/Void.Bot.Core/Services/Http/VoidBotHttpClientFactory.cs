using System.Net.Http.Headers;
using Autofac;
using Void.Bot.Core.Extension;
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
        var client = CreateClient(token, headers, timeout, beginScope);
        
        var response = await client.GetAsync<T>(url, cancellationToken).ConfigureAwait(false);

        return response;
    }

    public async Task<VoidBotResponse<T>> PostAsync<T>(string url, HttpContent content, CancellationToken cancellationToken, TimeSpan? timeout = null, TokenModel? token = null, Dictionary<string, string>? headers = null, bool beginScope = false)
    {
        var client = CreateClient(token, headers, timeout, beginScope);
        
        var response = await client.PostAsync<T>(url, content, cancellationToken).ConfigureAwait(false);

        return response;
    }

    public async Task<VoidBotResponse<T>> PostAsync<T>(string url, object content, CancellationToken cancellationToken, TimeSpan? timeout = null, TokenModel? token = null, Dictionary<string, string>? headers = null, bool beginScope = false)
    {
        var client = CreateClient(token, headers, timeout, beginScope);
        
        var response = await client.PostAsync<T>(url, content, cancellationToken).ConfigureAwait(false);

        return response;
    }
}