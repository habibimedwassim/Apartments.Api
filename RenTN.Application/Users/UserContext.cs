using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace RenTN.Application.Users;

public interface IUserContext
{
    CurrentUser? GetCurrentUser();
}
public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public CurrentUser? GetCurrentUser()
    {
        var user = httpContextAccessor?.HttpContext?.User;
        if (user == null) throw new InvalidOperationException("User context is not present");

        if (user.Identity == null || !user.Identity.IsAuthenticated) return null;

        var userId = user.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)!.Value;
        var email = user.FindFirst(x => x.Type == ClaimTypes.Email)!.Value;
        var roles = user.Claims.Where(x => x.Type == ClaimTypes.Role)!.Select(x => x.Value);

        return new CurrentUser(userId, email, roles);
    }
}

public record CurrentUser(string Id, string Email, IEnumerable<string> Roles)
{
    public bool IsInRole(string role) => Roles.Contains(role);
}