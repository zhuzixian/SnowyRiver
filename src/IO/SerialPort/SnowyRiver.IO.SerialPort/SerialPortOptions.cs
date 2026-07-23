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
    } = "utf-8";
}
