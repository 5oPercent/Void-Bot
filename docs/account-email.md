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
POST /api/identity/email/unbind
```

The status endpoint returns only a masked address. The account identifier comes from
the authenticated user and is never accepted from the request body. Unbinding requires
the current account password. It is idempotent and clears both the email address and its
verification timestamp, allowing the account to bind another email through the same
send-code and bind flow.

```json
{
  "password": "CurrentPassword1"
}
```

## Reset a password

```text
POST /api/identity/password-reset/send-code
POST /api/identity/password-reset
```

The send-code endpoint returns the same response shape whether an email exists or not.
Only accounts with a verified email can complete a reset. A successful password reset
increments the account token version, invalidating previously issued refresh tokens.

## Verification policy

Defaults are configured under `AccountEmail`:

```json
{
  "AccountEmail": {
    "CodeTtlMinutes": 10,
    "ResendIntervalSeconds": 60,
    "MaxSendsPerHour": 5,
    "MaxAttempts": 5
  }
}
```

Codes are six digits, protected with `ISecretProtector`, scoped by purpose, single-use,
and never written to logs. Email addresses are normalized before storage and masked in
logs and API status responses.
