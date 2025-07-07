using System.IO.Ports;

namespace SnowyRiver.Modbus.FluentModbus;
public interface IModbusRtuClient:IModbusClient
{
    public Task<TResult> ExecuteAsync<TResult>(Func<SerialPort?, CancellationToken, Task<TResult>> task,
        CancellationToken cancellationToken = default);
}
