using dixanh.Libraries.Entities;
using dixanh.Libraries.Models;
using Microsoft.AspNetCore.Identity;

namespace dixanh.Servers.Contracts;

public interface IAuthServer
{
    //Login
    Task<AppUser> Login(AppLoginDTO login);
    //Register
    Task<IdentityResult> Register(AppRegisterDTO register);

    //Create Token
    Task<string> CreateToken(InfomationUserSaveInToken user);

    //Get Role name
    Task<string> GetRoleName(AppUser user);

    //Get me
    Task<AppUser> GetMe(string userId);

    //Edit 
    Task<IdentityResult> EditMe(AppEditDTO models, string userId);
    //Delete
    Task<IdentityResult> DeleteMe(string userId);
    Task<IdentityResult> ChangeCurrentPassword(string userId, string oldPassword, string newPassword);
}