using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
namespace Apartments.Application.Utilities;

public class ClaimsPrincipalUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        string id = connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        return id;
    }
}
