using FluentModbus;
using Polly.Retry;
using System.Net;

namespace SnowyRiver.Modbus.FluentModbus;

public class RetryModbusRtuOverTcpClient(
    ModbusTcpClientOptions options, 
    AsyncRetryPolicy retryPolicy,
    ModbusRtuOverTcpClient modbusOverTcpClient)
    : RetryModbusTcpClientBase(options, retryPolicy, modbusOverTcpClient)
{
    private readonly ModbusRtuOverTcpClient _modbusRtuOverTcpClient = modbusOverTcpClient;
    public override void Close()
    {
        _modbusRtuOverTcpClient.Disconnect();
        base.Close();
    }

    protected override void Connect(IPEndPoint remoteEndpoint, ModbusEndianness endianness)
    {
        var tcpClient = CreateTcpClient(modbusOverTcpClient.ConnectTimeout, remoteEndpoint);
        modbusOverTcpClient.Initialize(tcpClient, endianness);
    }
}
