# Email service

Void Bot sends email through Microsoft Graph while exposing only the provider-neutral
`IEmailService` to business services.

Open the HTML files under
[`src/Void.Bot.Core/Services/Email/Templates`](../../src/Void.Bot.Core/Services/Email/Templates)
in a browser to preview the standard card and verification-code presentation used by
outgoing email. The files are embedded into `Void.Bot.Core` at build time.
For a side-by-side preview populated with all current business scenarios, open
[`email-template-scenarios-preview.html`](email-template-scenarios-preview.html).
System-generated email copy is provided by the `Email/Content` i18n context and uses
the language resolved from the current request, falling back to the default language.
Event-action subjects and content remain administrator-configured text.

## Configuration

Register a Microsoft Entra application that supports personal Microsoft accounts and
configure this redirect URI exactly:

```text
https://your-api.example.com/api/email/oauth/callback
```

The authorization flow requests delegated `Mail.Send` and `offline_access`. Configure:

```json
{
  "SecretProtection": {
    "EncryptionKey": "base64-encoded-32-byte-key"
  },
  "Email": {
    "ClientId": "application-client-id",
    "ClientSecret": "application-client-secret",
    "Tenant": "consumers",
    "RedirectUri": "https://your-api.example.com/api/email/oauth/callback",
    "ExpectedSenderEmail": "sender@outlook.com",
    "StateTtlSeconds": 600,
    "TokenRefreshIntervalDays": 30
  }
}
```

Generate the credential encryption key outside the repository, for example:

```shell
openssl rand -base64 32
```

`SecretProtection:EncryptionKey` is shared infrastructure for reversibly protected
application secrets. Each consumer supplies a purpose value so ciphertext from one
domain cannot be used by another. Provide the client secret and encryption key through
deployment secrets. Changing the encryption key without re-encrypting stored values
requires authorizing the sender mailbox again.

## Administration flow

1. Sign in as a Void Bot administrator.
2. Call `GET /api/email/oauth/authorize` and open the returned URL.
3. Sign in with the configured sender mailbox and grant consent. Microsoft redirects
   directly to the API callback, which returns the authorization result.
4. Check `GET /api/email/status`.
5. Call `POST /api/email/test` with a recipient, subject, and content.

If the provider revokes the refresh token, call
`POST /api/email/oauth/reauthorize` and open the returned URL. The existing credential
is retained until the replacement authorization succeeds.

The recurring job checks once per day and refreshes credentials whose 30-day refresh
date is due. Sending also refreshes an expired access token on demand.
