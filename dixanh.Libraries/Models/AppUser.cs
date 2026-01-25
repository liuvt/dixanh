using Microsoft.AspNetCore.Identity;

namespace dixanh.Libraries.Models;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Biography { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime? BirthDay { get; set; }
    public string Address { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }

}

//Register: set default role for new User
public partial class UserRoles
{
    public string RoleId { get; set; } = "5";
    public string RoleName { get; set; } = "Employee";
    public bool IsSelected { get; set; } = true;
}


//Login: create token 
public partial class InfomationUserSaveInToken
{
    public string id { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string userName { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string giveName { get; set; } = string.Empty;
    public string userRole { get; set; } = string.Empty;
    public string userGuiId { get; set; } = string.Empty;
}