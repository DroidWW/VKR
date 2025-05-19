using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace WebApplication1.Chathub
{
    public class UserIdProvider: IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
