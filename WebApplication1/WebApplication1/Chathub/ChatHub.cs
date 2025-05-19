using Microsoft.AspNetCore.SignalR;

namespace WebApplication1.Chathub
{
    public class ChatHub: Hub
    {
        public async Task SendMessage(int senderID, int recipientID, string message)
        {
            await Clients.Users(recipientID.ToString()).SendAsync("ReceiveMessage", senderID, message);
        }
    }
}
