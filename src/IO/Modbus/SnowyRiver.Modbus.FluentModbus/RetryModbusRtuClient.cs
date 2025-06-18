using FluentModbus;
using Polly.Retry;
using System.IO.Ports;

namespace SnowyRiver.Modbus.FluentModbus;

public class RetryModbusRtuClient(ModbusRtuClientOptions options, AsyncRetryPolicy retryPolicy,
    ModbusRtuClient modbusClient)
    :RetryModbusClient(modbusClient, retryPolicy)
{
    public void Connect()
    {
       Connect(options.PortName);
    }

    public void Connect(string port)
    {
        var serialPort = new SnowyRiverModbusRtuSerialPort(
            new SerialPort(port, options.BaudRate, options.Parity)
            {
                ReadTimeout = options.ReadTimeout,
                WriteTimeout = options.WriteTimeout,
            });
        Initialize(serialPort, options.Endian);
    }


    public void Initialize(IModbusRtuSerialPort serialPort, ModbusEndianness endianness)
    {
        modbusClient.Initialize(serialPort, endianness);
    }

    public void Close()
    {
        modbusClient.Close();
    }

}
