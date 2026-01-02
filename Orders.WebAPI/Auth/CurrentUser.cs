using System.Security.Claims;
using Order.Application.Interfaces;

namespace Orders.WebAPI.Auth;

public sealed class CurrentUser : ICurrentUser
{
    public readonly IHttpContextAccessor _accessor;
    
    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;
    
    public bool IsAuthenticated => _accessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public Guid UserId
    {
        get
        {
            var user = _accessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier);
            
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}