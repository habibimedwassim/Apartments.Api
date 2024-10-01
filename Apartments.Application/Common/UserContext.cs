using System.Security.Claims;
using Apartments.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace Apartments.Application.Common;

public interface IUserContext
{
    CurrentUser GetCurrentUser();
    bool IsUser();
}

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public CurrentUser GetCurrentUser()
    {
        var user = httpContextAccessor?.HttpContext?.User;
        if (user == null) throw new InvalidOperationException("User context is not present");

        if (user.Identity == null || !user.Identity.IsAuthenticated)
            throw new InvalidOperationException("User is not authenticated.");

        var userId = user.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)!.Value;
        var email = user.FindFirst(x => x.Type == ClaimTypes.Email)!.Value;
        var role = user.FindFirst(x => x.Type == ClaimTypes.Role)?.Value ?? UserRoles.User;
        var sysIdClaim = user.FindFirst(x => x.Type == ClaimTypes.Gender)?.Value;

        if (userId == null || email == null)
            throw new InvalidOperationException("User claims are missing required information.");

        if (string.IsNullOrEmpty(sysIdClaim) || !int.TryParse(sysIdClaim, out var sysId))
            throw new InvalidOperationException("User claims are missing required information for SysId.");

        return new CurrentUser(userId, email, sysId, role);
    }

    public bool IsUser()
    {
        var currentUser = GetCurrentUser();
        return currentUser.IsUser;
    }
}

public record CurrentUser(string Id, string Email, int SysId, string Role)
{
    public bool IsAdmin => Role.Equals(UserRoles.Admin, StringComparison.OrdinalIgnoreCase);
    public bool IsOwner => Role.Equals(UserRoles.Owner, StringComparison.OrdinalIgnoreCase);
    public bool IsUser => Role.Equals(UserRoles.User, StringComparison.OrdinalIgnoreCase);
}