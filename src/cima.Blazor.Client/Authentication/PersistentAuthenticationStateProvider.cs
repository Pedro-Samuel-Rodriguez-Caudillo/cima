using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace cima.Blazor.Client.Authentication;

public class PersistentAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly Task<AuthenticationState> _unauthenticatedTask =
        Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

    private readonly PersistentComponentState _state;
    private readonly HttpClient _httpClient;

    public PersistentAuthenticationStateProvider(PersistentComponentState state, HttpClient httpClient)
    {
        _state = state;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_state.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo) || userInfo is null)
        {
            userInfo = await FetchUserInfoFromConfigurationAsync();

            if (userInfo is null)
            {
                return await _unauthenticatedTask;
            }
        }

        return new AuthenticationState(new ClaimsPrincipal(CreateIdentity(userInfo)));
    }

    private async Task<UserInfo?> FetchUserInfoFromConfigurationAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApplicationConfigurationResponse>("api/abp/application-configuration");
            var currentUser = response?.CurrentUser;

            if (currentUser?.IsAuthenticated == true &&
                !string.IsNullOrWhiteSpace(currentUser.Id) &&
                !string.IsNullOrWhiteSpace(currentUser.UserName))
            {
                return new UserInfo
                {
                    UserId = currentUser.Id!,
                    UserName = currentUser.UserName!,
                    Email = currentUser.Email,
                    Roles = currentUser.Roles?.ToArray()
                };
            }
        }
        catch
        {
            // Ignorar y continuar como no autenticado para evitar romper el flujo de inicio de sesin
        }

        return null;
    }

    private static ClaimsIdentity CreateIdentity(UserInfo userInfo)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, userInfo.UserId),
            new Claim(ClaimTypes.Name, userInfo.UserName),
            new Claim(ClaimTypes.Email, userInfo.Email ?? string.Empty)
        ];

        if (userInfo.Roles != null)
        {
            foreach (var role in userInfo.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        return new ClaimsIdentity(claims, nameof(PersistentAuthenticationStateProvider));
    }
}

public class UserInfo
{
    public required string UserId { get; set; }
    public required string UserName { get; set; }
    public string? Email { get; set; }
    public string[]? Roles { get; set; }
}

internal sealed class ApplicationConfigurationResponse
{
    public CurrentUserInfo? CurrentUser { get; set; }
}

internal sealed class CurrentUserInfo
{
    public bool IsAuthenticated { get; set; }
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public IReadOnlyList<string>? Roles { get; set; }
}
