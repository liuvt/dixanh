using dixanh.Libraries.Models;
using dixanh.Libraries.Entities;
using Microsoft.AspNetCore.Components.Authorization;

namespace dixanh.Services.Interfaces;

public interface IAuthService
{
    //For user
    Task<AppUser> GetMe();
    Task<bool> DeleteMe();
    Task<bool> ChangePassword(AppChangePasswordDTO changePassword);
    Task<bool> EditMe(AppEditDTO models);
    Task<bool> Register(AppRegisterDTO models);
    Task Login(AppLoginDTO models);
    Task LogOut();
    Task<bool> CheckAuthenState();
    Task<AuthenticationState> GetAuthenState();
    //For Admin
}