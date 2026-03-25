using FluentModbus;
using Polly.Retry;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using SnowyRiver.Commons.Extensions;

namespace SnowyRiver.Modbus.FluentModbus;

public class RetryModbusTcpClient(
    ModbusTcpClientOptions options, 
    AsyncRetryPolicy retryPolicy,
    ModbusTcpClient modbusClient)
    :RetryModbusClient(modbusClient, retryPolicy),
        IModbusTcpClient
{

    public void Connect()
    {
        var endPoint = new IPEndPoint(IPAddress.Loopback, 502);
        if (options.RemoteEndpoint.IsNotNullOrWhiteSpace())
        {
            if (!TryParseEndpoint(options.RemoteEndpoint, out var parsedRemoteEndpoint))
                throw new FormatException("An invalid IPEndPoint was specified.");
            endPoint = parsedRemoteEndpoint;
        }
        Connect(endPoint, options.Endian);
    }

    protected void Connect(IPEndPoint remoteEndpoint, ModbusEndianness endianness)
    {
        var tcpClient = new TcpClient();
        if (!tcpClient.ConnectAsync(remoteEndpoint.Address, remoteEndpoint.Port)
                .Wait(modbusClient.ConnectTimeout))
        {
            throw new TimeoutException($"Failed to connect to the Modbus server at {remoteEndpoint} within the specified timeout of {modbusClient.ConnectTimeout} milliseconds.");
        }

        modbusClient.Initialize(tcpClient, endianness);
    }

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
