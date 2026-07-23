using System.IO.Ports;
using SnowyRiver.Configuration;

namespace SnowyRiver.IO.SerialPort;

public class SerialPortOptions : JsonConfiguration
{
    public bool IsMock
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string PortName
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public int BaudRate
    {
        get;
        set => SetProperty(ref field, value);
    } = 115200;

    public Parity Parity
    {
        get;
        set => SetProperty(ref field, value);
    } = Parity.None;

    public int DataBits
    {
        get;
        set => SetProperty(ref field, value);
    } = 8;

    public StopBits StopBits
    {
        get;
        set => SetProperty(ref field, value);
    } = StopBits.One;

    public int ReadTimeout
    {
        get;
        set => SetProperty(ref field, value);
    } = 1000;

    public int WriteTimeout
    {
        get;
        set => SetProperty(ref field, value);
    } = 1000;

    public string NewLine
    {
        get;
        set => SetProperty(ref field, value);
    } = "\r\n";

    /// <summary>
    /// 串口通信编码方式名称（如 "utf-8"、"ascii"、"gb2312"）。
    /// 默认为 "utf-8"。
    /// </summary>
    public string Encoding
    {
        get;
        set => SetProperty(ref field, value);
    } = "ascii";

    /// <summary>
    /// 串口握手协议。默认 None。
    /// </summary>
    public Handshake Handshake
    {
        get;
        set => SetProperty(ref field, value);
    } = Handshake.None;

    /// <summary>
    /// 是否启用 RTS（请求发送）信号。默认 false。
    /// </summary>
    public bool RtsEnable
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// 是否启用 DTR（数据终端就绪）信号。默认 false。
    /// </summary>
    public bool DtrEnable
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// 接收缓冲区大小（字节）。默认 4096。
    /// </summary>
    public int ReadBufferSize
    {
        get;
        set => SetProperty(ref field, value);
    } = 4096;

    /// <summary>
    /// 发送缓冲区大小（字节）。默认 2048。
    /// </summary>
    public int WriteBufferSize
    {
        get;
        set => SetProperty(ref field, value);
    } = 2048;
}
