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

    public async Task<string> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource(ReadTimeout);

        await using (timeoutCts.Token.Register(() =>
        {
            if (IsOpen) DiscardInBuffer();
        }))
        await using (cancellationToken.Register(() => timeoutCts.Cancel()))
        {
            var result = new System.Text.StringBuilder();
            var newLine = NewLine;
            var buffer = new byte[1];

            while (true)
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(cancellationToken);
                    }

                    if (timeoutCts.IsCancellationRequested)
                    {
                        throw new TimeoutException("The asynchronous read operation timed out.");
                    }

                    if (BytesToRead <= 0)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);
                        continue;
                    }


                    var bytesRead = await BaseStream.ReadAsync(buffer, 0, 1, timeoutCts.Token);
                    if (bytesRead == 0) continue;

                    var ch = Encoding.GetString(buffer, 0, 1);
                    result.Append(ch);

                    if (result.Length >= newLine.Length &&
                        result.ToString(result.Length - newLine.Length, newLine.Length) == newLine)
                    {
                        return result.ToString(0, result.Length - newLine.Length);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
                {
                    throw new TimeoutException("The asynchronous read operation timed out.");
                }
                catch (IOException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException("The asynchronous read operation timed out.");
                }
                catch (IOException)
                {
                    // 忽略并继续
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

    public async Task WriteLineAsync(string text, CancellationToken token = default)
    {
        var line = text + NewLine;
        var bytes = Encoding.GetBytes(line);
        await WriteAsync(bytes, 0, bytes.Length, token);
    }
}
