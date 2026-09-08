using System.Net;
using Serilog;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Resources.i18n.Email;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Commands.Email;
using Void.Bot.Message.Dto.Email;
using Void.Bot.Message.Request.Email;

namespace Void.Bot.Core.Services.Email;

public interface IEmailService : IScope
{
    Task<AuthorizeEmailResponse> AuthorizeAsync(
        AuthorizeEmailCommand command,
        CancellationToken cancellationToken);

    Task<CompleteEmailAuthorizationResponse> CompleteAuthorizationAsync(
        CompleteEmailAuthorizationCommand command,
        CancellationToken cancellationToken);

    Task<RefreshEmailCredentialResponse> RefreshCredentialAsync(
        RefreshEmailCredentialCommand command,
        CancellationToken cancellationToken);

    Task<GetEmailServiceStatusResponse> GetStatusAsync(
        GetEmailServiceStatusRequest request,
        CancellationToken cancellationToken);

    Task SendAsync(EmailMessageDto message, CancellationToken cancellationToken);

    Task<SendTestEmailResponse> SendTestAsync(
        SendTestEmailCommand command,
        CancellationToken cancellationToken);
}

internal sealed class EmailService(
    IEmailCredentialManager credentialManager,
    IMicrosoftGraphClient graphClient,
    ILocalizationService localizationService,
    ICurrentUser currentUser,
    ILogger logger) : IEmailService
{
    public Task<AuthorizeEmailResponse> AuthorizeAsync(
        AuthorizeEmailCommand command,
        CancellationToken cancellationToken)
    {
        return credentialManager.AuthorizeAsync(command, cancellationToken);
    }

    public Task<CompleteEmailAuthorizationResponse> CompleteAuthorizationAsync(
        CompleteEmailAuthorizationCommand command,
        CancellationToken cancellationToken)
    {
        return credentialManager.CompleteAuthorizationAsync(command, cancellationToken);
    }

    public Task<RefreshEmailCredentialResponse> RefreshCredentialAsync(
        RefreshEmailCredentialCommand command,
        CancellationToken cancellationToken)
    {
        return credentialManager.RefreshCredentialAsync(command, cancellationToken);
    }

    public Task<GetEmailServiceStatusResponse> GetStatusAsync(
        GetEmailServiceStatusRequest request,
        CancellationToken cancellationToken)
    {
        return credentialManager.GetStatusAsync(request, cancellationToken);
    }

    public async Task SendAsync(EmailMessageDto message, CancellationToken cancellationToken)
    {
        var outgoingMessage = ApplyStandardHtmlLayout(message);
        var accessToken = await credentialManager.GetAccessTokenAsync(
            cancellationToken).ConfigureAwait(false);
        try
        {
            await graphClient.SendAsync(accessToken, outgoingMessage, cancellationToken).ConfigureAwait(false);
        }
        catch (EmailServiceException exception) when (exception.ProviderStatusCode == HttpStatusCode.Unauthorized)
        {
            accessToken = await credentialManager.RefreshAccessTokenAsync(
                cancellationToken).ConfigureAwait(false);
            await graphClient.SendAsync(accessToken, outgoingMessage, cancellationToken).ConfigureAwait(false);
        }
    }

    private EmailMessageDto ApplyStandardHtmlLayout(EmailMessageDto message)
    {
        if (!string.IsNullOrWhiteSpace(message.HtmlContent))
            return message;

        return new EmailMessageDto
        {
            To = message.To,
            Subject = message.Subject,
            TextContent = message.TextContent,
            HtmlContent = EmailHtmlTemplate.Render(
                message.Subject,
                message.TextContent,
                GetTemplateText()),
            ReplyTo = message.ReplyTo
        };
    }

    private EmailTemplateText GetTemplateText()
    {
        return new EmailTemplateText(
            currentUser.Language.ToI18nCode(),
            localizationService.GetValue<EmailContentContextTemplate>(x => x.TeamSignature),
            localizationService.GetValue<EmailContentContextTemplate>(x => x.AutomatedNotice));
    }

    public async Task<SendTestEmailResponse> SendTestAsync(
        SendTestEmailCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            await SendAsync(
                new EmailMessageDto
                {
                    To = command.To,
                    Subject = command.Subject,
                    TextContent = command.Content
                },
                cancellationToken).ConfigureAwait(false);
            logger.Information("Test email accepted by the provider");
            return new SendTestEmailResponse { Message = "Test email accepted by the provider." };
        }
        catch (EmailReauthorizationRequiredException exception)
        {
            logger.Warning(
                exception,
                "Test email send requires reauthorization: {ErrorCode}",
                exception.ProviderErrorCode ?? "reauthorization_required");
            return new SendTestEmailResponse
            {
                Code = HttpStatusCode.Unauthorized,
                Message = "reauthorization_required"
            };
        }
        catch (EmailServiceException exception)
        {
            logger.Warning(
                exception,
                "Test email send failed: {ErrorCode}, provider status: {ProviderStatusCode}",
                exception.ProviderErrorCode ?? "email_send_failed",
                exception.ProviderStatusCode);
            return new SendTestEmailResponse
            {
                Code = HttpStatusCode.BadGateway,
                Message = exception.ProviderErrorCode ?? "email_send_failed"
            };
        }
    }
}
