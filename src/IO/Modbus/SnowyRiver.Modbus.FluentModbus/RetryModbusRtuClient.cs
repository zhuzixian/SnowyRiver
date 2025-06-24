using FluentModbus;
using Polly.Retry;
using System.IO.Ports;

namespace SnowyRiver.Modbus.FluentModbus;

public class RetryModbusRtuClient(ModbusRtuClientOptions options, AsyncRetryPolicy retryPolicy,
    ModbusRtuClient modbusClient)
    :RetryModbusClient(modbusClient, retryPolicy)
{
    private SerialPort? _serialPort;

    public void Connect()
    {
       Connect(options.PortName);
    }

    public void Connect(string port)
    {
        Connect(new SerialPort(port, options.BaudRate, options.Parity)
        {
            ReadTimeout = options.ReadTimeout,
            WriteTimeout = options.WriteTimeout,
            NewLine = options.NewLine,
        });
    }

    public void Connect(SerialPort port)
    {
        _serialPort = port;
        var serialPort = new SnowyRiverModbusRtuSerialPort(port);
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


    public Task<TResult> ExecuteAsync<TResult>(Func<SerialPort?, CancellationToken, Task<TResult>> task, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(async () => 
                await task.Invoke(_serialPort, cancellationToken), 
            cancellationToken);
    }
}
