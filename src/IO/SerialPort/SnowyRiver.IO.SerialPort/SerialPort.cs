using System.Buffers;
using System.Text;

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

    public SerialPort(SerialPortOptions options)
        : base(options.PortName, options.BaudRate, options.Parity, options.DataBits, options.StopBits)
    {
        ReadTimeout = options.ReadTimeout;
        WriteTimeout = options.WriteTimeout;
        NewLine = options.NewLine;
        // 如 SerialPortOptions 包含以下属性,一并应用(按需取消注释)
        // Handshake = options.Handshake;
        // RtsEnable = options.RtsEnable;
        // DtrEnable = options.DtrEnable;
        // if (options.Encoding is not null) Encoding = options.Encoding;
        // if (options.ReadBufferSize  > 0) ReadBufferSize  = options.ReadBufferSize;
        // if (options.WriteBufferSize > 0) WriteBufferSize = options.WriteBufferSize;
    }

    public async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var hasTimeout = ReadTimeout > 0;
        if (hasTimeout)
        {
            cts.CancelAfter(ReadTimeout);
        }
        try
        {
            await WaitToReadAsync(1, cts.Token);
            if (BytesToRead < count)
            {
                // 贪心法，等待后续数据到达一并读取
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(10), cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (hasTimeout && !cancellationToken.IsCancellationRequested)
                {
                    // 等数据时正好超时:按当前缓冲读出
                }
            }

            var readCount = Math.Min(BytesToRead, count);
            if (readCount <= 0) throw new TimeoutException(ReadTimeoutMessage);

            return await BaseStream.ReadAsync(buffer, offset, readCount, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (hasTimeout && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(ReadTimeoutMessage);
        }
        catch (IOException) when (hasTimeout && cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(ReadTimeoutMessage);
        }
    }

    public async Task<string> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        const int chunkSize = 64;
        var newLine = NewLine;
        var decoder = Encoding.GetDecoder();
        var result = new StringBuilder();

        var byteBuffer = ArrayPool<byte>.Shared.Rent(chunkSize);
        var charBuffer = ArrayPool<char>.Shared.Rent(Encoding.GetMaxCharCount(chunkSize));

        try
        {
            while (true)
            {
                var bytesRead = await ReadAsync(byteBuffer, 0, chunkSize, cancellationToken).ConfigureAwait(false);
                if (bytesRead == 0) continue;

                var charsDecoded = decoder.GetChars(byteBuffer, 0, bytesRead, charBuffer, 0, flush: false);
                if (charsDecoded <= 0) continue;

                result.Append(charBuffer, 0, charsDecoded);

                if (EndsWith(result, newLine))
                {
                    return result.ToString(0, result.Length - newLine.Length);
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(byteBuffer);
            ArrayPool<char>.Shared.Return(charBuffer);
        }

        static bool EndsWith(StringBuilder sb, string suffix)
        {
            if (sb.Length < suffix.Length) return false;
            var start = sb.Length - suffix.Length;
            for (var i = 0; i < suffix.Length; i++)
            {
                if (sb[start + i] != suffix[i]) return false;
            }
            return true;
        }
    }

    public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        if (!IsOpen) throw new InvalidOperationException("Serial port is not open.");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var hasTimeout = WriteTimeout > 0;
        if (hasTimeout) cts.CancelAfter(WriteTimeout);

        try
        {
            await BaseStream.WriteAsync(buffer, offset, count, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (hasTimeout && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(WriteTimeoutMessage);
        }
        catch (IOException) when (hasTimeout && cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(WriteTimeoutMessage);
        }
    }

    public async Task WriteLineAsync(string text, CancellationToken cancellationToken = default)
    {
        var newLine = NewLine;
        var maxByteCount = Encoding.GetMaxByteCount(text.Length + newLine.Length);
        var rented = ArrayPool<byte>.Shared.Rent(maxByteCount);

        try
        {
            var written = Encoding.GetBytes(text, 0, text.Length, rented, 0);
            written += Encoding.GetBytes(newLine, 0, newLine.Length, rented, written);
            await WriteAsync(rented, 0, written, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    protected async Task WaitToReadAsync(int count, CancellationToken cancellationToken = default)
    {
        while (BytesToRead < count && IsOpen)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
        }
        if (!IsOpen) throw new InvalidOperationException("Serial port is not open.");
    }
}
