using System.Net;

namespace Void.Bot.Core.Services.Email;

internal static class EmailHtmlTemplate
{
    private const string ResourcePrefix = "Void.Bot.Core.Services.Email.Templates";
    private static readonly Lazy<string> StandardTemplate = new(() => LoadTemplate("StandardEmail.html"));
    private static readonly Lazy<string> VerificationCodeTemplate = new(() => LoadTemplate("VerificationCodeEmail.html"));

    public static string Render(string subject, string content, EmailTemplateText templateText)
    {
        return StandardTemplate.Value
            .Replace("{{Subject}}", Encode(subject), StringComparison.Ordinal)
            .Replace("{{Content}}", Encode(content), StringComparison.Ordinal)
            .ReplaceTemplateText(templateText);
    }

    public static string RenderVerificationCode(
        string subject,
        string introduction,
        string code,
        string expirationNotice,
        EmailTemplateText templateText)
    {
        return VerificationCodeTemplate.Value
            .Replace("{{Subject}}", Encode(subject), StringComparison.Ordinal)
            .Replace("{{Introduction}}", Encode(introduction), StringComparison.Ordinal)
            .Replace("{{Code}}", Encode(code), StringComparison.Ordinal)
            .Replace("{{ExpirationNotice}}", Encode(expirationNotice), StringComparison.Ordinal)
            .ReplaceTemplateText(templateText);
    }

    private static string Encode(string value)
    {
        return WebUtility.HtmlEncode(value);
    }

    private static string ReplaceTemplateText(this string template, EmailTemplateText templateText)
    {
        return template
            .Replace("{{LanguageCode}}", Encode(templateText.LanguageCode), StringComparison.Ordinal)
            .Replace("{{TeamSignature}}", Encode(templateText.TeamSignature), StringComparison.Ordinal)
            .Replace("{{AutomatedNotice}}", Encode(templateText.AutomatedNotice), StringComparison.Ordinal);
    }

    private static string LoadTemplate(string fileName)
    {
        var resourceName = $"{ResourcePrefix}.{fileName}";
        using var stream = typeof(EmailHtmlTemplate).Assembly.GetManifestResourceStream(resourceName) ??
                           throw new InvalidOperationException($"Embedded email template '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

internal sealed record EmailTemplateText(
    string LanguageCode,
    string TeamSignature,
    string AutomatedNotice);
