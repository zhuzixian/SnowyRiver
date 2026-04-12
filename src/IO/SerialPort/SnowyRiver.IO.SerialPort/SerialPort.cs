namespace SnowyRiver.IO.SerialPort;

public class SerialPort : System.IO.Ports.SerialPort, ISerialPort
{
    private const string ReadTimeoutMessage = "The read operation timed out.";
    private const string WriteTimeoutMessage = "The write operation timed out.";

    public SerialPort()
    { }

    public SerialPort(System.ComponentModel.IContainer container) : base(container) { }

    public SerialPort(string portName) : base(portName) { }

    public SerialPort(string portName, int baudRate) : base(portName, baudRate) { }

    public SerialPort(string portName, int baudRate, System.IO.Ports.Parity parity) : base(portName, baudRate, parity) { }

    public SerialPort(string portName, int baudRate, System.IO.Ports.Parity parity, int dataBits) : base(portName, baudRate, parity, dataBits) { }

    public SerialPort(string portName, int baudRate, System.IO.Ports.Parity parity, int dataBits, System.IO.Ports.StopBits stopBits)
        : base(portName, baudRate, parity, dataBits, stopBits) { }


    public async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken token = default)
    {
        using var timeoutCts = new CancellationTokenSource(ReadTimeout);
        await using var cancellationRegistration = token.Register(timeoutCts.Cancel);
        try
        {
            await WaitToReadAsync(1, timeoutCts.Token);
            var readCount = Math.Min(BytesToRead, count);
            if (readCount <= 0)
            {
                throw new TimeoutException(ReadTimeoutMessage);
            }

            if (readCount < count)
            {
                // 贪心法，等待后续数据到达一并读取
                await Task.Delay(TimeSpan.FromMilliseconds(10), token);
            }

            return await BaseStream.ReadAsync(buffer, offset, readCount, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            throw new TimeoutException(ReadTimeoutMessage);
        }
        catch (IOException) when (timeoutCts.IsCancellationRequested && !token.IsCancellationRequested)
        {
            throw new TimeoutException(ReadTimeoutMessage);
        }
    }

    public async Task<string> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        var result = new System.Text.StringBuilder();
        var newLine = NewLine;
        var byteBuffer = new byte[1];
        var decoder = Encoding.GetDecoder();
        var charBuffer = new char[Encoding.GetMaxCharCount(1)];

        while (true)
        {
            var bytesRead = await ReadAsync(byteBuffer, 0, 1, cancellationToken);
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
    }

    public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token = default)
    {
        using var timeoutCts = new CancellationTokenSource(WriteTimeout);
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
            throw new TimeoutException(WriteTimeoutMessage);
        }
        catch (IOException) when (timeoutCts.IsCancellationRequested && !token.IsCancellationRequested)
        {
            throw new TimeoutException(WriteTimeoutMessage);
        }
    }

    public async Task WriteLineAsync(string text, CancellationToken token = default)
    {
        var line = text + NewLine;
        var bytes = Encoding.GetBytes(line);
        await WriteAsync(bytes, 0, bytes.Length, token);
    }

    protected async Task WaitToReadAsync(int count, CancellationToken token = default)
    {
        while (BytesToRead < count && IsOpen && !token.IsCancellationRequested)
        {
            await Task.Delay(1, CancellationToken.None);
        }
    }
}
