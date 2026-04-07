using System.IO.Ports;
using FluentModbus;
using Polly;
using SnowyRiver.Modbus.FluentModbus;

namespace SnowyRiver.IO.Modbus.FluentModbus.Tests;

public class RetryModbusRtuClientTests
{
    [Fact]
    public void Test_Connect()
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(retryCount: 5, _ => TimeSpan.FromMilliseconds(1));
        var portName = System.IO.Ports.SerialPort.GetPortNames().LastOrDefault();
        Assert.NotNull(portName);
        var modbusRtuClient = new RetryModbusRtuClient(new ModbusRtuClientOptions
            {
                PortName = portName,
                BaudRate = 9600,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Endian = ModbusEndianness.BigEndian,
                ReadTimeout = 2000
            }, retryPolicy,
            new ModbusRtuClient());
        modbusRtuClient.Connect();
    }
}
