using System.Collections;
using System.Reflection;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Middlewares.Logger;

internal static class LogRedactor
{
    private const string Redacted = "[REDACTED]";

    public static object? Redact(object? value)
    {
        return Redact(value, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    public static bool ContainsSensitiveValues(object? value)
    {
        return ContainsSensitiveValues(value, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    private static bool ContainsSensitiveValues(object? value, HashSet<object> visited)
    {
        if (value is null or string)
        {
            return false;
        }

        var type = value.GetType();
        if (type.IsPrimitive || type.IsEnum || value is decimal or Guid or DateTime or DateTimeOffset or TimeSpan)
        {
            return false;
        }

        if (!type.IsValueType && !visited.Add(value))
        {
            return false;
        }

        if (value is IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                var name = entry.Key?.ToString() ?? string.Empty;
                if (IsSensitiveName(name, type) || ContainsSensitiveValues(entry.Value, visited))
                {
                    return true;
                }
            }

            return false;
        }

        if (value is IEnumerable enumerable)
        {
            return enumerable.Cast<object?>().Any(item => ContainsSensitiveValues(item, visited));
        }

        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.CanRead && property.GetIndexParameters().Length == 0)
            .Any(property => IsSensitiveName(property.Name, type) ||
                             ContainsSensitiveValues(property.GetValue(value), visited));
    }

    private static object? Redact(object? value, HashSet<object> visited)
    {
        if (value is null)
        {
            return null;
        }

        if (value is string text)
        {
            return LooksLikeJwt(text) ? Redacted : text;
        }

        var type = value.GetType();
        if (type.IsPrimitive || type.IsEnum || value is decimal or Guid or DateTime or DateTimeOffset or TimeSpan)
        {
            return value;
        }

        if (!type.IsValueType && !visited.Add(value))
        {
            return "[CIRCULAR]";
        }

        if (value is IDictionary dictionary)
        {
            var redactedDictionary = new Dictionary<string, object?>();
            foreach (DictionaryEntry entry in dictionary)
            {
                var name = entry.Key?.ToString() ?? string.Empty;
                redactedDictionary[name] = IsUniversallySensitiveName(name)
                    ? Redacted
                    : Redact(entry.Value, visited);
            }

            return redactedDictionary;
        }

        if (value is IEnumerable enumerable)
        {
            return enumerable.Cast<object?>()
                .Select(item => Redact(item, visited))
                .ToArray();
        }

        var result = new Dictionary<string, object?>();
        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                     .Where(property => property.CanRead && property.GetIndexParameters().Length == 0))
        {
            result[property.Name] = IsSensitiveName(property.Name, type)
                ? Redacted
                : Redact(property.GetValue(value), visited);
        }

        return result;
    }

    private static bool IsSensitiveName(string name, Type ownerType)
    {
        if (IsUniversallySensitiveName(name))
        {
            return true;
        }

        var normalized = Normalize(name);
        if (normalized == "code" &&
            ownerType.Name.Contains("TwoFactor", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!IsOAuthContainer(ownerType))
        {
            return false;
        }

        return normalized is "state" or "errordescription" or "authorizationurl" or "redirecturl" ||
               normalized == "code" && !typeof(VoidBotResponse).IsAssignableFrom(ownerType);
    }

    private static bool IsUniversallySensitiveName(string name)
    {
        var normalized = Normalize(name);
        return normalized.Contains("password", StringComparison.Ordinal) ||
               normalized.Contains("recoverycode", StringComparison.Ordinal) ||
               normalized is "accountsecret" or "jwt" or "twofactorcode" ||
               normalized.EndsWith("token", StringComparison.Ordinal) ||
               normalized.Contains("secret", StringComparison.Ordinal);
    }

    private static bool IsOAuthContainer(Type type)
    {
        return type.Name.Contains("OAuth", StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string name)
    {
        return string.Concat(name.Where(char.IsLetterOrDigit)).ToLowerInvariant();
    }

    private static bool LooksLikeJwt(string value)
    {
        var segments = value.Split('.');
        return segments.Length == 3 &&
               segments.All(segment => segment.Length > 0 && segment.All(IsBase64UrlCharacter));
    }

    private static bool IsBase64UrlCharacter(char character)
    {
        return char.IsAsciiLetterOrDigit(character) || character is '-' or '_';
    }
}
