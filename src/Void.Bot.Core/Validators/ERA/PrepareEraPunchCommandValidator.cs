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
            .Must(addresses => addresses != null && addresses.Count <= 32 && addresses.All(IsUsableIpv4));
        RuleFor(message => message.LocalPort)
            .Must((message, port) => message.LocalAddresses != null &&
                (message.LocalAddresses.Count == 0 ? port == 0 : port is >= 1 and <= 65535));
    }

    private static bool IsUsableIpv4(string value)
    {
        if (!IPAddress.TryParse(value, out var address) || address.AddressFamily != AddressFamily.InterNetwork)
        {
            return false;
        }
        var bytes = address.GetAddressBytes();
        // 接受有效网卡 IPv4（包含 VPN），排除未指定、回环、链路本地、组播和保留地址。
        return bytes[0] != 0 && bytes[0] != 127 && bytes[0] < 224 &&
            !(bytes[0] == 169 && bytes[1] == 254);
    }
}
