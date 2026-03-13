using FluentModbus;
using SnowyRiver.IO.SerialPort;

namespace SnowyRiver.Modbus.FluentModbus;

public class ModbusRtuSerialPort(string portName, int baudRate, System.IO.Ports.Parity parity)
    : SerialPort(portName, baudRate, parity), IModbusRtuSerialPort;
