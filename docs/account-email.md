# Account email

Account email binding is optional. An omitted email never blocks registration or login.
When an email is supplied, it is saved only after the corresponding verification
challenge succeeds.

## Registration

Request a code:

```http
POST /api/identity/register/email/send-code
Content-Type: application/json

{"email":"user@example.com"}
```

Register without an email by omitting `emailVerification`. To bind an email during
registration, include the challenge returned by the send-code endpoint:

```json
{
  "name": "User",
  "password": "Password1",
  "emailVerification": {
    "email": "user@example.com",
    "challengeId": "00000000-0000-0000-0000-000000000000",
    "code": "123456"
  }
}
```

`OAuthRegisterCommand` accepts the same optional `emailVerification` object.

## Bind an existing account

Authenticated users can call:

```text
GET  /api/identity/email/status
POST /api/identity/email/send-code
POST /api/identity/email/bind
POST /api/identity/email/unbind/send-code
POST /api/identity/email/unbind
```

The status endpoint returns only a masked address. The account identifier comes from
the authenticated user and is never accepted from the request body. To unbind an email,
first call the unbind send-code endpoint without a request body. The verification code is
sent only to the current account's verified email address. Submit the returned challenge
identifier and the six-digit code to the unbind endpoint:

```json
{
  "challengeId": "00000000-0000-0000-0000-000000000000",
  "code": "123456"
}
```

The unbinding challenge is scoped to the authenticated user, bound email, and `Unbinding`
purpose. It uses the same expiration, resend interval, hourly send limit, failed-attempt
limit, and single-use policy as the other email verification flows. Successful unbinding
clears both the email address and its verification timestamp, allowing the account to bind
another email through the same send-code and bind flow.

## Reset a password

```text
POST /api/identity/password-reset/send-code
POST /api/identity/password-reset
```

The send-code endpoint returns the same response shape whether an email exists or not.
Only accounts with a verified email can complete a reset. A successful password reset
increments the account token version, invalidating previously issued refresh tokens.

Administrators with the corresponding endpoint permission can send a one-time reset link:

```text
POST /api/account/password/reset-link
POST /api/identity/password-reset/link
```

The first endpoint accepts a `userId`, requires a verified account email, invalidates any
previous link for that account, and sends a 256-bit single-use token in the URL fragment.
The anonymous completion endpoint accepts `token`, `newPassword`, and
`newPasswordConfirm`. Completing the reset increments the account token version. Link
delivery and completion do not publish EventSystem events.

## Verification policy

Defaults are configured under `AccountEmail`:

```json
{
  "AccountEmail": {
    "CodeTtlMinutes": 10,
    "ResendIntervalSeconds": 60,
    "MaxSendsPerHour": 5,
    "MaxAttempts": 5,
    "PasswordResetLinkTtlMinutes": 15,
    "PasswordResetFrontendUrl": "https://voidbot.example/password-reset"
  }
}
```

Codes are six digits, protected with `ISecretProtector`, scoped by purpose, single-use,
and never written to logs. Email addresses are normalized before storage and masked in
logs and API status responses.
