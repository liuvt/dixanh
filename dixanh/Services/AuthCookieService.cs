using dixanh.Libraries.Models;
using dixanh.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace dixanh.Services;
public class AuthCookieService : IAuthCookieService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly UserManager<AppUser> _userManager;

    public AuthCookieService(AuthenticationStateProvider authStateProvider, UserManager<AppUser> userManager)
    {
        _authStateProvider = authStateProvider;
        _userManager = userManager;
    }

    public async Task<AppUser?> GetMeAsync()
    {
        var state = await _authStateProvider.GetAuthenticationStateAsync();
        var user = state.User;
        if (user.Identity?.IsAuthenticated != true) return null;

        var userId = _userManager.GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId)) return null;

        return await _userManager.FindByIdAsync(userId);
    }

}