using dixanh.Libraries.Entities;

namespace dixanh.Services.Interfaces
{
    public interface IAuthJwtService
    {
        Task<string> LoginTokenAsync(AppLoginDTO dto);
        Task LogoutTokenAsync();
    }
}
