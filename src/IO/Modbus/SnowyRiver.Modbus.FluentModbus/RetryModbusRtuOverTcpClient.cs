using FluentModbus;
using Polly.Retry;
using SnowyRiver.Commons.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
namespace SnowyRiver.Modbus.FluentModbus;

public class RetryModbusRtuOverTcpClient(
    ModbusTcpClientOptions options, 
    AsyncRetryPolicy retryPolicy,
    ModbusRtuOverTcpClient modbusOverTcpClient)
    : RetryModbusTcpClientBase(options, retryPolicy, modbusOverTcpClient)
{
    protected override void Connect(IPEndPoint remoteEndpoint, ModbusEndianness endianness)
    {
        var tcpClient = CreateTcpClient(modbusOverTcpClient.ConnectTimeout, remoteEndpoint);
        modbusOverTcpClient.Initialize(tcpClient, endianness);
    }
}
