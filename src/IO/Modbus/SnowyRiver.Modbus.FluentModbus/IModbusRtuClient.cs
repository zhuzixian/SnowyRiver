using SnowyRiver.IO.SerialPort;

namespace SnowyRiver.Modbus.FluentModbus;
public interface IModbusRtuClient:IModbusClient
{
    public void Close();
    public Task<TResult> ExecuteAsync<TResult>(Func<ISerialPort?, CancellationToken, Task<TResult>> task,
        CancellationToken cancellationToken = default);
}
