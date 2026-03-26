namespace SnowyRiver.IO.SerialPort;

public class SerialPort : System.IO.Ports.SerialPort,ISerialPort
{
    private const string AsyncReadTimeoutMessage = "The asynchronous read operation timed out.";
    private const string AsyncWriteTimeoutMessage = "The asynchronous write operation timed out.";

    public SerialPort()
    { }

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
        await using var timeoutRegistration = timeoutCts.Token.Register(() => {
            if (!IsOpen && !DiscardInBufferOnReadTimeout)
            {
                return;
            }

            try
            {
                DiscardInBuffer();
            }
            catch (InvalidOperationException)
            {
            }
        });
        await using var cancellationRegistration = token.Register(timeoutCts.Cancel);

        try
        {
            return await BaseStream.ReadAsync(buffer, offset, count, timeoutCts.Token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            throw new TimeoutException(AsyncReadTimeoutMessage);
        }
        catch (IOException) when (timeoutCts.IsCancellationRequested && !token.IsCancellationRequested)
        {
            throw new TimeoutException(AsyncReadTimeoutMessage);
        }
    }

    public async Task<string> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource(ReadTimeout);

        await using var cancellationRegistration = cancellationToken.Register(timeoutCts.Cancel);

        var result = new System.Text.StringBuilder();
        var newLine = NewLine;
        var byteBuffer = new byte[1];
        var decoder = Encoding.GetDecoder();
        var charBuffer = new char[Encoding.GetMaxCharCount(1)];

        try
        {
            while (true)
            {
                try
                {
                    var bytesRead = await BaseStream.ReadAsync(byteBuffer, 0, 1, timeoutCts.Token);
                    if (bytesRead == 0) continue;

                    var charsDecoded = decoder.GetChars(byteBuffer, 0, bytesRead, charBuffer, 0, flush: false);
                    if (charsDecoded <= 0)
                    {
                        continue;
                    }

                    result.Append(charBuffer, 0, charsDecoded);

                    if (result.Length >= newLine.Length &&
                        result.ToString(result.Length - newLine.Length, newLine.Length) == newLine)
                    {
                        return result.ToString(0, result.Length - newLine.Length);
                    }
                }
                catch (IOException)
                {
                    if (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                    {
                        throw new TimeoutException(AsyncReadTimeoutMessage);
                    }

                    throw;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            throw new TimeoutException(AsyncReadTimeoutMessage);
        }
    }

    public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token = default)
    {
        // https://github.com/AndreasAmMueller/Modbus/blob/a6d11080c2f5a1205681c881f3ba163d2ac84a1f/src/Modbus.Serial/Util/Extensions.cs#L69
        // https://stackoverflow.com/a/54610437/11906695
        // https://github.com/dotnet/runtime/issues/28968
        using var timeoutCts = new CancellationTokenSource(WriteTimeout);

        /* _serialPort.DiscardInBuffer is essential here to cancel the operation */
        await using var timeoutRegistration = timeoutCts.Token.Register(() =>
        {
            if (!IsOpen && !DiscardOutBufferOnWriteTimeout)
            {
                return;
            }

            try
            {
                DiscardOutBuffer();
            }
            catch (InvalidOperationException)
            {
            }
        });
        await using var cancellationRegistration = token.Register(timeoutCts.Cancel);

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
            throw new TimeoutException(AsyncWriteTimeoutMessage);
        }
        catch (IOException) when (timeoutCts.IsCancellationRequested && !token.IsCancellationRequested)
        {
            throw new TimeoutException(AsyncWriteTimeoutMessage);
        }
    }

    public async Task WriteLineAsync(string text, CancellationToken token = default)
    {
        var line = text + NewLine;
        var bytes = Encoding.GetBytes(line);
        await WriteAsync(bytes, 0, bytes.Length, token);
    }

    public virtual bool DiscardOutBufferOnWriteTimeout => false;
    public virtual bool DiscardInBufferOnReadTimeout => false;
}
