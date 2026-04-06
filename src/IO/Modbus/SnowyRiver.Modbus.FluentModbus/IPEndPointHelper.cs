using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;

namespace SnowyRiver.Modbus.FluentModbus;

internal static class IpEndPointHelper
{
    public static bool TryParseEndpoint(ReadOnlySpan<char> value, [NotNullWhen(true)] out IPEndPoint? result)
    {
        var addressLength = value.Length;
        var lastColonPos = value.LastIndexOf(':');

        if (lastColonPos > 0)
        {
            if (value[lastColonPos - 1] == ']')
                addressLength = lastColonPos;

            else if (value[..lastColonPos].LastIndexOf(':') == -1)
                addressLength = lastColonPos;
        }

        if (IPAddress.TryParse(value[..addressLength].ToString(), out var address))
        {
            var port = 502U;

            if (addressLength == value.Length ||
                (uint.TryParse(value[(addressLength + 1)..].ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out port) && port <= 65536))

            {
                result = new IPEndPoint(address, (int)port);
                return true;
            }
        }

        result = default;

        return false;
    }
}
