using System.IO.Ports;

namespace SnowyRiver.Modbus.FluentModbus;
public interface IModbusRtuClient:IModbusClient
{
    public void Close();
    public Task<TResult> ExecuteAsync<TResult>(Func<SerialPort?, CancellationToken, Task<TResult>> task,
        bool isUpdateLastAccessTime = true,
        CancellationToken cancellationToken = default);
    public Task ExecuteAsync(Func<SerialPort?, CancellationToken, Task> task,
        bool isUpdateLastAccessTime = true,
        CancellationToken cancellationToken = default);
}
