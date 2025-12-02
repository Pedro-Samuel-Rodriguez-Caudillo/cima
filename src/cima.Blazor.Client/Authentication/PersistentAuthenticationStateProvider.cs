using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace cima.Blazor.Client.Authentication;

/// <summary>
/// AuthenticationStateProvider para Blazor WebAssembly que recibe el estado
/// del servidor a través de PersistentComponentState.
/// </summary>
public class PersistentAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly Task<AuthenticationState> _unauthenticatedTask =
        Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

    private readonly Task<AuthenticationState> _authenticationStateTask = _unauthenticatedTask;

    public PersistentAuthenticationStateProvider(PersistentComponentState persistentComponentState)
    {
        if (!persistentComponentState.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo) || 
            userInfo is null)
        {
            return;
        }

        Claim[] claims = [
            new Claim(ClaimTypes.NameIdentifier, userInfo.UserId),
            new Claim(ClaimTypes.Name, userInfo.UserName),
            new Claim(ClaimTypes.Email, userInfo.Email ?? string.Empty)
        ];

        if (userInfo.Roles != null)
        {
            foreach (var role in userInfo.Roles)
            {
                claims = [.. claims, new Claim(ClaimTypes.Role, role)];
            }
        }

        _authenticationStateTask = Task.FromResult(
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, 
                authenticationType: nameof(PersistentAuthenticationStateProvider)))));
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return _authenticationStateTask;
    }
}

/// <summary>
/// Información del usuario que se persiste desde el servidor al cliente WebAssembly.
/// </summary>
public class UserInfo
{
    public required string UserId { get; set; }
    public required string UserName { get; set; }
    public string? Email { get; set; }
    public string[]? Roles { get; set; }
}
