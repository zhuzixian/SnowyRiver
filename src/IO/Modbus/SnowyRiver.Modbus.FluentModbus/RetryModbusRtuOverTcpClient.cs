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
    public override void Close()
    {
        using (AsyncLocker.Lock())
        {
            modbusOverTcpClient.Disconnect();
            base.Close();
        }
    }

    protected override void Connect(IPEndPoint remoteEndpoint, ModbusEndianness endianness)
    {
        using (AsyncLocker.Lock())
        {
            try { modbusOverTcpClient.Disconnect(); } catch { /* ignore*/ }
            var tcpClient = CreateTcpClient(modbusOverTcpClient.ConnectTimeout, remoteEndpoint);
            modbusOverTcpClient.Initialize(tcpClient, endianness);
        }
    }
}
