using System.Net;
using System.Net.Sockets;
using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class PrepareEraPunchCommandValidator : FluentMessageValidator<PrepareEraPunchCommand>
{
    public PrepareEraPunchCommandValidator()
    {
        RuleFor(message => message.AuthorityEpoch)
            .GreaterThan(0);
        RuleFor(message => message.TeamId)
            .NotEmpty();
        RuleFor(message => message.MemberId)
            .NotEmpty();
        RuleFor(message => message.LocalAddresses)
            .Must(addresses => addresses != null && addresses.Count <= 8 && addresses.All(IsPrivateIpv4));
        RuleFor(message => message.LocalPort)
            .Must((message, port) => message.LocalAddresses != null &&
                (message.LocalAddresses.Count == 0 ? port == 0 : port is >= 1 and <= 65535));
    }

    private static bool IsPrivateIpv4(string value)
    {
        if (!IPAddress.TryParse(value, out var address) || address.AddressFamily != AddressFamily.InterNetwork)
        {
            return false;
        }
        var bytes = address.GetAddressBytes();
        return bytes[0] == 10 ||
            (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
            (bytes[0] == 192 && bytes[1] == 168);
    }
}
