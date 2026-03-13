using SnowyRiver.IO.SerialPort;

namespace SnowyRiver.Modbus.FluentModbus;
public interface IModbusRtuClient:IModbusClient
{
    public void Close();
    public Task<TResult> ExecuteAsync<TResult>(Func<ISerialPort?, CancellationToken, Task<TResult>> task,
        bool isUpdateLastAccessTime = true,
        CancellationToken cancellationToken = default);
    public Task ExecuteAsync(Func<ISerialPort?, CancellationToken, Task> task,
        bool isUpdateLastAccessTime = true,
        CancellationToken cancellationToken = default);
}
