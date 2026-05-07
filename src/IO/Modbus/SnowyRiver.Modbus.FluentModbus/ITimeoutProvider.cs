using System;
using System.Collections.Generic;
using System.Text;

namespace SnowyRiver.Modbus.FluentModbus;

public interface ITimeoutProvider
{
    public TimeSpan Timeout { get; set; }
}
