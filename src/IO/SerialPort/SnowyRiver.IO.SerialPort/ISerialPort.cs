namespace SnowyRiver.IO.SerialPort;

public interface ISerialPort : IDisposable
{
    int BaudRate { get; set; }
    bool BreakState { get; set; }
    int BytesToRead { get; }
    int BytesToWrite { get; }
    bool CDHolding { get; }
    bool CtsHolding { get; }
    int DataBits { get; set; }
    bool DiscardNull { get; set; }
    bool DsrHolding { get; }
    bool DtrEnable { get; set; }
    System.Text.Encoding Encoding { get; set; }
    System.IO.Ports.Handshake Handshake { get; set; }
    bool IsOpen { get; }
    string NewLine { get; set; }
    System.IO.Ports.Parity Parity { get; set; }
    byte ParityReplace { get; set; }
    string PortName { get; set; }
    int ReadBufferSize { get; set; }
    int ReadTimeout { get; set; }
    int ReceivedBytesThreshold { get; set; }
    bool RtsEnable { get; set; }
    System.IO.Ports.StopBits StopBits { get; set; }
    int WriteBufferSize { get; set; }
    int WriteTimeout { get; set; }

    void Open();
    void Close();
    void DiscardInBuffer();
    void DiscardOutBuffer();
    public int Read(byte[] buffer, int offset, int count);
    int Read(char[] buffer, int offset, int count);
    int ReadByte();
    int ReadChar();
    string ReadExisting();
    string ReadLine();
    string ReadTo(string value);
    void Write(string text);
    void Write(byte[] buffer, int offset, int count);
    void Write(char[] buffer, int offset, int count);
    void WriteLine(string text);

    Task<string> ReadLineAsync(CancellationToken cancellationToken = default);
    Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken token = default);
    Task WriteLineAsync(string text, CancellationToken cancellationToken = default);
    Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token = default);
}
