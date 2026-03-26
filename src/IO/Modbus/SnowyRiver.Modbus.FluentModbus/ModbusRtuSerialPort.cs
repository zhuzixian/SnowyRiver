using FluentModbus;

namespace SnowyRiver.Modbus.FluentModbus;

public class ModbusRtuSerialPort(string portName, int baudRate, System.IO.Ports.Parity parity)
    : IO.SerialPort.SerialPort(portName, baudRate, parity), IModbusRtuSerialPort
{
    public override bool DiscardOutBufferOnWriteTimeout => true;
    public override bool DiscardInBufferOnReadTimeout => true;
}
