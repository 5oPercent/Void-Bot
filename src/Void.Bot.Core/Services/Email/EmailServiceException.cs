using System.Net;

namespace Void.Bot.Core.Services.Email;

public class EmailServiceException : Exception
{
    public string? ProviderErrorCode { get; }

    public HttpStatusCode? ProviderStatusCode { get; }

    public EmailServiceException(
        string message,
        Exception? innerException = null,
        string? providerErrorCode = null,
        HttpStatusCode? providerStatusCode = null)
        : base(message, innerException)
    {
        ProviderErrorCode = providerErrorCode;
        ProviderStatusCode = providerStatusCode;
    }
}

internal sealed class EmailReauthorizationRequiredException : EmailServiceException
{
    public EmailReauthorizationRequiredException(
        string? providerErrorCode = null,
        HttpStatusCode? providerStatusCode = null)
        : base(
            "The email account must be authorized again.",
            providerErrorCode: providerErrorCode,
            providerStatusCode: providerStatusCode)
    {
    }
}

internal sealed class EmailProviderUnavailableException : EmailServiceException
{
    public EmailProviderUnavailableException(
        string providerErrorCode,
        Exception innerException)
        : base(
            "The email provider is unavailable.",
            innerException,
            providerErrorCode)
    {
    }
}
