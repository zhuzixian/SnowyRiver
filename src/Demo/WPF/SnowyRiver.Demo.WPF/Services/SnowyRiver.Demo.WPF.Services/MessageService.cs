using SnowyRiver.Demo.WPF.Services.Interfaces;

namespace SnowyRiver.Demo.WPF.Services
{
    public class MessageService : IMessageService
    {
        public string GetMessage()
        {
            return "Hello from the Message Service";
        }
    }
}
