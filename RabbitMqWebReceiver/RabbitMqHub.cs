using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace RabbitMqWebReceiver
{
    public class RabbitMqHub : Hub
    {
        public async Task Send(string message, string userName)
        {
            await Clients.All.SendAsync("Receive", message, userName);
        }
    }
}