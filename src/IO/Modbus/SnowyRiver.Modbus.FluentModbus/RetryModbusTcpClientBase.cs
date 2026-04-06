using FluentModbus;
using Polly.Retry;
using SnowyRiver.Commons.Extensions;
using System.Net;
using System.Net.Sockets;

namespace SnowyRiver.Modbus.FluentModbus;

public abstract class RetryModbusTcpClientBase(
    ModbusTcpClientOptions options, 
    AsyncRetryPolicy retryPolicy,
    ModbusClient modbusClient)
    :RetryModbusClient(modbusClient, retryPolicy)
{

    public void Connect()
    {
        var endPoint = new IPEndPoint(IPAddress.Loopback, 502);
        if (options.RemoteEndpoint.IsNotNullOrWhiteSpace())
        {
            if (!IpEndPointHelper.TryParseEndpoint(options.RemoteEndpoint, out var parsedRemoteEndpoint))
                throw new FormatException("An invalid IPEndPoint was specified.");
            endPoint = parsedRemoteEndpoint;
        }
        Connect(endPoint, options.Endian);
    }

    protected abstract void Connect(IPEndPoint remoteEndpoint, ModbusEndianness endianness);

    protected TcpClient CreateTcpClient(int connectTimeout, IPEndPoint remoteEndpoint)
    {
        var tcpClient = new TcpClient();
        if (!tcpClient.ConnectAsync(remoteEndpoint.Address, remoteEndpoint.Port)
                .Wait(connectTimeout))
        {
            throw new TimeoutException($"Failed to connect to the Modbus server at {
                remoteEndpoint} within the specified timeout of {connectTimeout} milliseconds.");
        }

        return tcpClient;
    }
}
