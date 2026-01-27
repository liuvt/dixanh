using dixanh.Libraries.Entities;
using dixanh.Libraries.Models;
using Microsoft.AspNetCore.Identity;

namespace dixanh.Servers.Contracts;

public interface IAuthServer
{
    // Cookie (web nội bộ)
    Task<AppUser> LoginCookie(AppLoginDTO login);
    Task LogoutCookie();

    // JWT (SPA/Mobile/Postman)
    Task<string> LoginJwt(AppLoginDTO login);

    // Account
    Task<IdentityResult> Register(AppRegisterDTO register);

    // Profile
    Task<AppUser> GetMe(string userId);
    Task<IdentityResult> EditMe(AppEditDTO models, string userId);
    Task<IdentityResult> DeleteMe(string userId);
    Task<IdentityResult> ChangeCurrentPassword(string userId, string currentPassword, string newPassword);
}