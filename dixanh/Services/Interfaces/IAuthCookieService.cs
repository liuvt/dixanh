using dixanh.Libraries.Models;

namespace dixanh.Services.Interfaces;

public interface IAuthCookieService
{
    Task<AppUser?> GetMeAsync();
}