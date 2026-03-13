namespace SnowyRiver.IO.SerialPort;

public class SerialPort : System.IO.Ports.SerialPort,ISerialPort
{

    public SerialPort():base() { }

    public SerialPort(System.ComponentModel.IContainer container) : base(container) { }

    public SerialPort(string portName):base(portName) { }

    public SerialPort(string portName, int baudRate):base(portName, baudRate) { }

    public SerialPort(string portName, int baudRate, System.IO.Ports.Parity parity) : base(portName, baudRate, parity){}

    public SerialPort(string portName, int baudRate, System.IO.Ports.Parity parity, int dataBits): base(portName, baudRate, parity, dataBits) { }

    public SerialPort(string portName, int baudRate, System.IO.Ports.Parity parity, int dataBits, System.IO.Ports.StopBits stopBits)
        :base(portName, baudRate, parity, dataBits, stopBits) { }


    public async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken token = default)
    {
        // https://github.com/AndreasAmMueller/Modbus/blob/a6d11080c2f5a1205681c881f3ba163d2ac84a1f/src/Modbus.Serial/Util/Extensions.cs#L69
        // https://stackoverflow.com/a/54610437/11906695
        // https://github.com/dotnet/runtime/issues/28968

        using var timeoutCts = new CancellationTokenSource(ReadTimeout);

        /* _serialPort.DiscardInBuffer is essential here to cancel the operation */
        await using (timeoutCts.Token.Register(() =>
                     {
                         if (IsOpen) DiscardInBuffer();
                     }))
        await using (token.Register(() => timeoutCts.Cancel()))
        {
            while (true)
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(token);
                    }

                    if (timeoutCts.IsCancellationRequested)
                    {
                        throw new TimeoutException("The asynchronous read operation timed out.");
                    }

                    if (BytesToRead <= 0)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1), token);
                        continue;
                    }

                    return await BaseStream.ReadAsync(buffer, offset, count, timeoutCts.Token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    throw;
                }
                catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
                {
                    throw new TimeoutException("The asynchronous read operation timed out.");
                }
                catch (IOException) when (timeoutCts.IsCancellationRequested && !token.IsCancellationRequested)
                {
                    throw new TimeoutException("The asynchronous read operation timed out.");
                }
                catch (IOException)
                {
                    //
                }
            }

        }
    }

    public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token = default)
    {
        // https://github.com/AndreasAmMueller/Modbus/blob/a6d11080c2f5a1205681c881f3ba163d2ac84a1f/src/Modbus.Serial/Util/Extensions.cs#L69
        // https://stackoverflow.com/a/54610437/11906695
        // https://github.com/dotnet/runtime/issues/28968
        using var timeoutCts = new CancellationTokenSource(WriteTimeout);

        /* _serialPort.DiscardInBuffer is essential here to cancel the operation */
        await using (timeoutCts.Token.Register(DiscardOutBuffer))
        await using (token.Register(() => timeoutCts.Cancel()))
        {
            try
            {
                await BaseStream.WriteAsync(buffer, offset, count, timeoutCts.Token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                throw new TimeoutException("The asynchronous write operation timed out.");
            }
            catch (IOException) when (timeoutCts.IsCancellationRequested && !token.IsCancellationRequested)
            {
                throw new TimeoutException("The asynchronous write operation timed out.");
            }
        }
    }
}
