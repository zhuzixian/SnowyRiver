using System.Buffers;
using System.Text;

namespace SnowyRiver.IO.SerialPort;

public class SerialPort : System.IO.Ports.SerialPort, ISerialPort
{
    private const string ReadTimeoutMessage = "The read operation timed out.";
    private const string WriteTimeoutMessage = "The write operation timed out.";

    private int _readTimeoutMs;
    private int _writeTimeoutMs;

    // 串口属于半双工的访问语义,且共享同一底层句柄,读写共用同一把锁,任意时刻只允许一个 IO 操作
    private readonly SemaphoreSlim _ioLock = new(1, 1);
    private bool _disposed;

    // 非 Options 构造函数的默认超时值（毫秒）
    private const int DefaultReadTimeoutMs = 1000;
    private const int DefaultWriteTimeoutMs = 1000;
    // 贪心读取等待延迟（毫秒），用于等待后续数据到达一并读取
    private const int GreedyReadDelayMs = 10;
    // 轮询等待间隔（毫秒）
    private const int PollingIntervalMs = 1;
    // ReadLineAsync 每次读取的字节块大小
    private const int ReadLineChunkSize = 64;

    public SerialPort()
    {
        InitializeDefaults();
    }

    public SerialPort(System.ComponentModel.IContainer container) : base(container)
    {
        InitializeDefaults();
    }

    public SerialPort(string portName) : base(portName)
    {
        InitializeDefaults();
    }

    public SerialPort(string portName, int baudRate) : base(portName, baudRate)
    {
        InitializeDefaults();
    }

    public SerialPort(string portName, int baudRate, System.IO.Ports.Parity parity) : base(portName, baudRate, parity)
    {
        InitializeDefaults();
    }

    public SerialPort(string portName, int baudRate, System.IO.Ports.Parity parity, int dataBits) : base(portName, baudRate, parity, dataBits)
    {
        InitializeDefaults();
    }

    public SerialPort(string portName, int baudRate, System.IO.Ports.Parity parity, int dataBits, System.IO.Ports.StopBits stopBits)
        : base(portName, baudRate, parity, dataBits, stopBits)
    {
        InitializeDefaults();
    }

    public SerialPort(SerialPortOptions options)
        : base(options.PortName, options.BaudRate, options.Parity, options.DataBits, options.StopBits)
    {
        // 自己用 CancellationToken 做超时,基类不要再下发 SET_TIMEOUTS
        ReadTimeout = InfiniteTimeout;
        WriteTimeout = InfiniteTimeout;

        // 把用户在 options 里配置的超时存到自己的字段,给 ReadAsync/WriteAsync 用
        _readTimeoutMs = options.ReadTimeout;
        _writeTimeoutMs = options.WriteTimeout;

        NewLine = options.NewLine;
        if (!string.IsNullOrEmpty(options.Encoding))
            Encoding = System.Text.Encoding.GetEncoding(options.Encoding);
        // 应用 SerialPortOptions 中的扩展配置
        Handshake = options.Handshake;
        RtsEnable = options.RtsEnable;
        DtrEnable = options.DtrEnable;
        if (options.ReadBufferSize > 0) ReadBufferSize = options.ReadBufferSize;
        if (options.WriteBufferSize > 0) WriteBufferSize = options.WriteBufferSize;
    }

    public async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var hasTimeout = _readTimeoutMs > 0;
        if (hasTimeout)
        {
            cts.CancelAfter(_readTimeoutMs);
        }
        try
        {
            // 获取 IO 锁,串口不允许读写并发
            await _ioLock.WaitAsync(cts.Token).ConfigureAwait(false);
            try
            {
                await WaitToReadAsync(1, cts.Token).ConfigureAwait(false);
                if (BytesToRead < count)
                {
                    // 贪心法，等待后续数据到达一并读取
                    try
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(GreedyReadDelayMs), cts.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (hasTimeout && !cancellationToken.IsCancellationRequested)
                    {
                        // 等数据时正好超时:按当前缓冲读出
                    }
                }

                var readCount = Math.Min(BytesToRead, count);
                if (readCount <= 0) throw new TimeoutException(ReadTimeoutMessage);

                return await BaseStream.ReadAsync(buffer, offset, readCount, cts.Token).ConfigureAwait(false);
            }
            finally
            {
                _ioLock.Release();
            }
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
        var newLine = NewLine;
        var decoder = Encoding.GetDecoder();
        var result = new StringBuilder();

        var byteBuffer = ArrayPool<byte>.Shared.Rent(ReadLineChunkSize);
        var charBuffer = ArrayPool<char>.Shared.Rent(Encoding.GetMaxCharCount(ReadLineChunkSize));

        try
        {
            while (true)
            {
                var bytesRead = await ReadAsync(byteBuffer, 0, ReadLineChunkSize, cancellationToken).ConfigureAwait(false);
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

    // ===== 以下同步方法均使用 _ioLock 保护，确保与异步操作互斥 =====

    /// <summary>
    /// [线程安全] 读取指定数量的字节到缓冲区，与异步读写操作互斥。
    /// </summary>
    public new int Read(byte[] buffer, int offset, int count)
    {
        _ioLock.Wait();
        try
        {
            return base.Read(buffer, offset, count);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    /// <summary>
    /// [线程安全] 读取指定数量的字符到缓冲区，与异步读写操作互斥。
    /// </summary>
    public new int Read(char[] buffer, int offset, int count)
    {
        _ioLock.Wait();
        try
        {
            return base.Read(buffer, offset, count);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    /// <summary>
    /// [线程安全] 读取一个字节，与异步读写操作互斥。
    /// </summary>
    public new int ReadByte()
    {
        _ioLock.Wait();
        try
        {
            return base.ReadByte();
        }
        finally
        {
            _ioLock.Release();
        }
    }

    /// <summary>
    /// [线程安全] 读取一个字符，与异步读写操作互斥。
    /// </summary>
    public new int ReadChar()
    {
        _ioLock.Wait();
        try
        {
            return base.ReadChar();
        }
        finally
        {
            _ioLock.Release();
        }
    }

    /// <summary>
    /// [线程安全] 读取当前缓冲区中所有可用的数据，与异步读写操作互斥。
    /// </summary>
    public new string ReadExisting()
    {
        _ioLock.Wait();
        try
        {
            return base.ReadExisting();
        }
        finally
        {
            _ioLock.Release();
        }
    }

    /// <summary>
    /// [线程安全] 读取一行（直到 NewLine），与异步读写操作互斥。
    /// </summary>
    public new string ReadLine()
    {
        _ioLock.Wait();
        try
        {
            return base.ReadLine();
        }
        finally
        {
            _ioLock.Release();
        }
    }

    /// <summary>
    /// [线程安全] 读取直到指定分隔符，与异步读写操作互斥。
    /// </summary>
    public new string ReadTo(string value)
    {
        _ioLock.Wait();
        try
        {
            return base.ReadTo(value);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    /// <summary>
    /// [线程安全] 写入字节数组，与异步读写操作互斥。
    /// </summary>
    public new void Write(byte[] buffer, int offset, int count)
    {
        _ioLock.Wait();
        try
        {
            base.Write(buffer, offset, count);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    /// <summary>
    /// [线程安全] 写入字符串，与异步读写操作互斥。
    /// </summary>
    public new void Write(string text)
    {
        _ioLock.Wait();
        try
        {
            base.Write(text);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    /// <summary>
    /// [线程安全] 写入字符数组，与异步读写操作互斥。
    /// </summary>
    public new void Write(char[] buffer, int offset, int count)
    {
        _ioLock.Wait();
        try
        {
            base.Write(buffer, offset, count);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    /// <summary>
    /// [线程安全] 写入一行（附加 NewLine），与异步读写操作互斥。
    /// </summary>
    public new void WriteLine(string text)
    {
        _ioLock.Wait();
        try
        {
            base.WriteLine(text);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    /// <summary>
    /// [线程安全] 清空接收缓冲区，与异步读写操作互斥。
    /// </summary>
    public new void DiscardInBuffer()
    {
        _ioLock.Wait();
        try
        {
            base.DiscardInBuffer();
        }
        finally
        {
            _ioLock.Release();
        }
    }

    /// <summary>
    /// [线程安全] 清空发送缓冲区，与异步读写操作互斥。
    /// </summary>
    public new void DiscardOutBuffer()
    {
        _ioLock.Wait();
        try
        {
            base.DiscardOutBuffer();
        }
        finally
        {
            _ioLock.Release();
        }
    }

    /// <summary>
    /// [线程安全] 打开串口连接，与异步读写操作互斥。
    /// </summary>
    public new void Open()
    {
        _ioLock.Wait();
        try
        {
            base.Open();
        }
        finally
        {
            _ioLock.Release();
        }
    }

    /// <summary>
    /// [线程安全] 关闭串口连接，与异步读写操作互斥。
    /// </summary>
    public new void Close()
    {
        _ioLock.Wait();
        try
        {
            base.Close();
        }
        finally
        {
            _ioLock.Release();
        }
    }

    public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        if (!IsOpen) throw new InvalidOperationException("Serial port is not open.");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var hasTimeout = _writeTimeoutMs > 0;
        if (hasTimeout) cts.CancelAfter(_writeTimeoutMs);

        try
        {
            // 获取 IO 锁,串口不允许读写并发
            await _ioLock.WaitAsync(cts.Token).ConfigureAwait(false);
            try
            {
                await BaseStream.WriteAsync(buffer, offset, count, cts.Token).ConfigureAwait(false);
            }
            finally
            {
                _ioLock.Release();
            }
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
            await Task.Delay(PollingIntervalMs, cancellationToken).ConfigureAwait(false);
        }
        if (!IsOpen) throw new InvalidOperationException("Serial port is not open.");
    }

    /// <summary>
    /// 为非 Options 构造函数设置默认超时值，确保 ReadAsync/WriteAsync 超时机制正常工作。
    /// </summary>
    private void InitializeDefaults()
    {
        ReadTimeout = InfiniteTimeout;
        WriteTimeout = InfiniteTimeout;
        _readTimeoutMs = DefaultReadTimeoutMs;
        _writeTimeoutMs = DefaultWriteTimeoutMs;
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _ioLock.Dispose();
            }
            _disposed = true;
        }
        base.Dispose(disposing);
    }
}
