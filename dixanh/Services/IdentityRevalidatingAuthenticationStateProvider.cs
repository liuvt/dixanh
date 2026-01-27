using dixanh.Libraries.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace dixanh.Services;
public sealed class IdentityRevalidatingAuthenticationStateProvider
    : RevalidatingServerAuthenticationStateProvider
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<IdentityOptions> _identityOptions;

    public IdentityRevalidatingAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory scopeFactory,
        IOptions<IdentityOptions> identityOptions)
        : base(loggerFactory)
    {
        _scopeFactory = scopeFactory;
        _identityOptions = identityOptions;
    }

    // Set the revalidation interval to 30 minutes
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    // Validate the authentication state
    // This method is called periodically based on the RevalidationInterval
    // to ensure the user's authentication state is still valid
    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        var principal = authenticationState.User;
        if (principal.Identity?.IsAuthenticated != true)
            return false;

        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var userId = userManager.GetUserId(principal);
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return false;

        // SecurityStamp revalidate
        if (userManager.SupportsUserSecurityStamp)
        {
            var stampType = _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType;
            var principalStamp = principal.FindFirstValue(stampType);
            var dbStamp = await userManager.GetSecurityStampAsync(user);
            return principalStamp == dbStamp;
        }

        return true;
    }
}
