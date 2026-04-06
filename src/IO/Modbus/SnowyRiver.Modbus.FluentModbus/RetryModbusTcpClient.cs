using FluentModbus;
using Polly.Retry;
using System.Net;
using System.Net.Sockets;

namespace SnowyRiver.Modbus.FluentModbus;

public class RetryModbusTcpClient(
    ModbusTcpClientOptions options, 
    AsyncRetryPolicy retryPolicy,
    ModbusTcpClient modbusClient)
    : RetryModbusTcpClientBase(options, retryPolicy, modbusClient),IModbusTcpClient
{
    protected override void Connect(IPEndPoint remoteEndpoint, ModbusEndianness endianness)
    {
        var tcpClient = CreateTcpClient(modbusClient.ConnectTimeout, remoteEndpoint);
        modbusClient.Initialize(tcpClient, endianness);
    }
}
