using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Serilog;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Extension;

public static class HttpClientExtension
{
    extension(HttpClient client)
    {
        public async Task<VoidBotResponse<T>> GetAsync<T>(string url, CancellationToken cancellationToken)
        {
            return await SafeProcessAsync(url, async () =>
            {
                var response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);

                var data = response.IsSuccessStatusCode
                    ? await ReadResponseContentAsAsync<T>(response, cancellationToken).ConfigureAwait(false)
                    : default;
                var message = response.IsSuccessStatusCode
                    ? string.Empty
                    : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            
                return new VoidBotResponse<T>
                {
                    Code = response.StatusCode,
                    Data = data,
                    Message = message
                };
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task<VoidBotResponse<T>> PostAsync<T>(string url, HttpContent content, CancellationToken cancellationToken)
        {
            return await SafeProcessAsync(url, async () =>
            {
                var response = await client.PostAsync(url, content, cancellationToken).ConfigureAwait(false);

                var data = response.IsSuccessStatusCode
                    ? await ReadResponseContentAsAsync<T>(response, cancellationToken).ConfigureAwait(false)
                    : default;
                var message = response.IsSuccessStatusCode
                    ? string.Empty
                    : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            
                return new VoidBotResponse<T>
                {
                    Code = response.StatusCode,
                    Data = data,
                    Message = message
                };
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task<VoidBotResponse<T>> PostAsync<T>(string url, object content, CancellationToken cancellationToken)
        {
            return await SafeProcessAsync(url, async () =>
            {
                var response = await client.PostAsJsonAsync(url, content, cancellationToken).ConfigureAwait(false);

                var data = response.IsSuccessStatusCode
                    ? await ReadResponseContentAsAsync<T>(response, cancellationToken).ConfigureAwait(false)
                    : default;
                var message = response.IsSuccessStatusCode
                    ? string.Empty
                    : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                return new VoidBotResponse<T>
                {
                    Code = response.StatusCode,
                    Data = data,
                    Message = message
                };
            }, cancellationToken).ConfigureAwait(false);
        }
    }


    public static async Task<VoidBotResponse<T>> SafeProcessAsync<T>(string requestUrl, Func<Task<VoidBotResponse<T>>> func, CancellationToken cancellationToken, bool shouldLogError = true)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            return await func().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (shouldLogError)
                Log.Error(ex, "Error on requesting {RequestUrl}", requestUrl);
            else
                Log.Warning(ex, "Error on requesting {RequestUrl}", requestUrl);
            
            return new VoidBotResponse<T>
            {
                Code = HttpStatusCode.BadRequest,
                Message = ex.Message
            };
        }
    }
    
    public static async Task<T> ReadResponseContentAsAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (typeof(T) == typeof(string))
            return (T)(object) await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        
        if (typeof(T) == typeof(byte))
            return (T)(object) await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        
        if (typeof(T) == typeof(Stream))
            return (T)(object) await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
            return default!;

        var content = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        if (content.Length == 0 || content.All(x => x is 0x20 or 0x09 or 0x0A or 0x0D))
            return default!;

        return JsonSerializer.Deserialize<T>(content, JsonSerializerOptions.Web)
               ?? throw new InvalidOperationException();
    }
}