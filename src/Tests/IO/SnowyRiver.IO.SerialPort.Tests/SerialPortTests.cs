namespace SnowyRiver.IO.SerialPort.Tests;

public class SerialPortTests
{
    [Fact]
    public async Task Test_ReadAsync_Timeout()
    {
        var port = new SerialPort("Com7", 115200);
        port.Open();
        var buffer = new byte[102];
        port.ReadTimeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;
        await Assert.ThrowsAsync<TimeoutException>(
            () => port.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None));
    }
}
