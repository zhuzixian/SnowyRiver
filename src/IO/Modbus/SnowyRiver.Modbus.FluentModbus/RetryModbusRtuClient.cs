using FluentModbus;
using Polly.Retry;
using SnowyRiver.IO.SerialPort;

namespace SnowyRiver.Modbus.FluentModbus;

public class RetryModbusRtuClient(
    ModbusRtuClientOptions options, 
    AsyncRetryPolicy retryPolicy,
    ModbusRtuClient modbusClient)
    :RetryModbusClient(modbusClient, retryPolicy),
        IModbusRtuClient
{
    private ISerialPort? _serialPort;

    public void Connect()
    {
       Connect(options.PortName);
    }

    public void Connect(string port)
    {
        Connect(new ModbusRtuSerialPort(port, options.BaudRate, options.Parity)
        {
            ReadTimeout = options.ReadTimeout,
            WriteTimeout = options.WriteTimeout,
            NewLine = options.NewLine,
        });
    }

    public void Connect(ModbusRtuSerialPort port)
    {
        _serialPort = port;
        Initialize(port, options.Endian);
    }


    public void Initialize(IModbusRtuSerialPort serialPort, ModbusEndianness endianness)
    {
        modbusClient.Initialize(serialPort, endianness);
    }

    public void Close()
    {
        modbusClient.Close();
    }


    public Task<TResult> ExecuteAsync<TResult>(Func<ISerialPort?, CancellationToken, Task<TResult>> task, 
        bool isUpdateLastAccessTime = true,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(async () => 
                await task.Invoke(_serialPort, cancellationToken), 
                isUpdateLastAccessTime, cancellationToken);
    }

    public Task ExecuteAsync(Func<ISerialPort?, CancellationToken, Task> task, bool isUpdateLastAccessTime = true, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(async () =>
                await task.Invoke(_serialPort, cancellationToken),
            isUpdateLastAccessTime, cancellationToken);
    }
}
