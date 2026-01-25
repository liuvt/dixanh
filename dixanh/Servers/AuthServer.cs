using dixanh.Libraries.Entities;
using dixanh.Libraries.Models;
using dixanh.Servers.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace dixanh.Servers;

public class AuthServer : IAuthServer
{

    //User Manager
    protected readonly UserManager<AppUser> userManager;
    protected readonly SignInManager<AppUser> loginManager;
    protected readonly IConfiguration configuration;

    //Constructor
    public AuthServer(UserManager<AppUser> _userManager, SignInManager<AppUser> _loginManager,
                        IConfiguration _configuration)
    {
        userManager = _userManager;
        loginManager = _loginManager;
        configuration = _configuration;
    }

    /* Lấy thông tin bản thân */
    public async Task<AppUser> GetMe(string userId)
        => await userManager.FindByIdAsync(userId) ?? throw new Exception("Lỗi người dùng vui lòng đăng nhập lại!");

    /* Xóa tài khoản */
    public async Task<IdentityResult> DeleteMe(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) throw new Exception("Lỗi người dùng vui lòng đăng nhập lại!");
        var result = await userManager.DeleteAsync(user);
        return result;
    }

    /* Đăng nhập */
    public async Task<AppUser> Login(AppLoginDTO login)
    {
        try
        {
            //Kiểm tra email
            var user = await userManager.FindByEmailAsync(login.Email);
            if (user == null)
                throw new Exception("Wrong Email or Password");

            //Đăng nhập với Email và Password
            var result = await loginManager.PasswordSignInAsync(
                                                login.Email, login.Password, false, false);
            if (!result.Succeeded)
                throw new Exception("Wrong Email or Password");

            return user;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /* Đăng ký */
    public async Task<IdentityResult> Register(AppRegisterDTO register)
    {
        //Kiểm tra email
        var user = await userManager.FindByEmailAsync(register.Email);
        if (user != null)
            throw new Exception("Email đã tồn tại");

        var newUser = new AppUser
        {
            Email = register.Email,
            FirstName = register.FirstName,
            LastName = register.LastName,
            UserName = register.Email,
            Gender = register.Gender,
            PublishedAt = DateTime.Now
        };

        //Create password
        var createUser = await userManager.CreateAsync(newUser, register.Password);

        //Kiểm tra trạng thái đăng ký thành công
        if (!createUser.Succeeded)
            throw new Exception("Đăng ký không thành công vui lòng làm lại!");
        else
        {
            #region Set role USER mặt định cho người dùng nếu đăng ký thành công
            //Khởi tạo IEnumerable<string> roles của hàm AddToRolesAsync
            List<UserRoles> roles = new List<UserRoles>();
            //Mặt định khi tạo mới tài khoản là quyền User
            var userRoles = new UserRoles() { RoleId = "5", RoleName = "Employee", IsSelected = true };
            roles.Add(userRoles);

            //Đăng ký role cho user vừa được tạo trên bảng aspnetuserroles [Database]
            await userManager.AddToRolesAsync(newUser,
                            roles.Where(x => x.IsSelected).Select(y => y.RoleName));
            #endregion

            //Đăng ký thành công trả về thông tin IdentityResult
            return createUser;
        }
    }

    /* Cập nhật */
    public async Task<IdentityResult> EditMe(AppEditDTO models, string userId)
    {
        var user = await userManager.FindByIdAsync(userId) ?? throw new Exception("Lỗi người dùng vui lòng đăng nhập lại");

        user.FirstName = models.FirstName;
        user.LastName = models.LastName;
        user.Biography = models.Biography;
        user.PhoneNumber = models.PhoneNumber;
        user.Address = models.Address;
        user.Gender = models.Gender;
        user.BirthDay = models.BirthDay;

        return await userManager.UpdateAsync(user);
    }

    /* Change currentPassword*/
    public async Task<IdentityResult> ChangeCurrentPassword(string userId, string currentPassword, string newPassword)
    {
        var user = await userManager.FindByIdAsync(userId) ?? throw new Exception("Lỗi người dùng vui lòng đăng nhập lại");

        var changePassword = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!changePassword.Succeeded) throw new Exception("Mật khẩu củ không đúng");

        return changePassword;
    }

    /* Tạo token*/
    public async Task<string> CreateToken(InfomationUserSaveInToken user)
    {
        try
        {
            //Thông tin User đưa vào Token
            var listClaims = new List<Claim>
                        {
                            new Claim("id", user.id),
                            new Claim("username", user.userName),
                            new Claim("email", user.email),
                            new Claim("name", user.name),
                            new Claim("give_name", user.giveName),
                            new Claim(ClaimTypes.Role, user.userRole),
                            new Claim(JwtRegisteredClaimNames.Jti, user.userGuiId)
                        };

            //Khóa bí mật
            var autKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]
                ?? throw new InvalidOperationException("Can't found [Secret Key] in appsettings.json !")));

            //Tạo chữ ký với khóa bí mật
            var signCredentials = new SigningCredentials(autKey, SecurityAlgorithms.HmacSha512Signature);

            var autToken = new JwtSecurityToken(
                claims: listClaims, //Thông tin User
                issuer: configuration["JWT:ValidIssuer"], //In file appsetting.json
                audience: configuration["JWT:ValidAudience"], //In file appsetting.json
                expires: DateTime.Now.AddDays(7), //Thời gian tồn tại Token
                signingCredentials: signCredentials //Chữ ký
            );

            return new JwtSecurityTokenHandler().WriteToken(autToken);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /* Lấy thông tin quyền truy cập */
    public async Task<string> GetRoleName(AppUser user)
    {
        try
        {
            //Lấy Role của User
            var userRoles = await userManager.GetRolesAsync(user);
            var rolename = userRoles.Select(e => e).FirstOrDefault();
            return rolename == null ? string.Empty : rolename;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

}