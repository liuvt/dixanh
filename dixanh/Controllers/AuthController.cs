using dixanh.Libraries.Entities;
using dixanh.Libraries.Models;
using dixanh.Servers.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace dixanh.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    //Get API Server
    private readonly IAuthServer context;
    private readonly ILogger<AuthController> logger;
    public AuthController(IAuthServer _context, ILogger<AuthController> _logger)
    {
        this.context = _context;
        this.logger = _logger;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(AppRegisterDTO register)
    {
        try
        {
            var result = await this.context.Register(register);
            if (!result.Succeeded) return Unauthorized();

            return Ok(result.Succeeded);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost("Login")]
    public async Task<ActionResult<string>> Login(AppLoginDTO appLogin)
    {
        try
        {
            //Login
            var appUser = await this.context.Login(appLogin);
            if (appUser == null)
                throw new Exception("Wrong Email or Password");

            //Role
            var role = await this.context.GetRoleName(appUser);

            //Create token
            var userClaim = new InfomationUserSaveInToken()
            {
                id = appUser.Id ?? string.Empty,
                email = appUser.Email ?? string.Empty,
                name = $"{appUser.FirstName} {appUser.LastName}" ?? string.Empty,
                giveName = $"{appUser.FirstName} {appUser.LastName}" ?? string.Empty,
                userName = appUser.UserName ?? string.Empty,
                userRole = role,
                userGuiId = Guid.NewGuid().ToString()
            };

            var token = await this.context.CreateToken(userClaim);

            return Ok(token);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                                                                "Error: " + ex.Message);
        }
    }

    [HttpGet("Me"), Authorize]
    public async Task<ActionResult<AppUser>> GetMe()
    {
        try
        {
            /*
                Console.WriteLine("User name: " + User.FindFirstValue(ClaimTypes.NameIdentifier)); //User name
                Console.WriteLine("Email: " + User.FindFirstValue(email));        //Email
                Console.WriteLine("Role: " + User.FindFirstValue(ClaimTypes.Role));             //Role
                Console.WriteLine("User Id: " + User.FindFirstValue("id"));    //User Id
            */
            return Ok(await this.context.GetMe(this.User.FindFirstValue("id")
                                                    ?? throw new Exception("Not found User ID")));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                                                                "Error: " + ex.Message);
        }
    }

    [HttpPatch("Me/Edit"), Authorize]
    public async Task<IActionResult> EditMe(AppEditDTO models)
    {
        try
        {
            var edit = await this.context.EditMe(models, this.User.FindFirstValue("id")
                                                                ?? throw new Exception("Not found User ID"));
            if (!edit.Succeeded) return BadRequest();

            return Ok(edit.Succeeded);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                                                                "Error: " + ex.Message);
        }
    }

    [HttpPatch("Me/ChangePassword"), Authorize]
    public async Task<IActionResult> ChangeCurrentPassword(AppChangePasswordDTO changePassword)
    {
        try
        {
            var userId = this.User.FindFirstValue("id")
                                        ?? throw new Exception("Not found User ID");
            var resetpassword = await this.context.ChangeCurrentPassword(userId, changePassword.CurrentPassword, changePassword.Password);
            if (!resetpassword.Succeeded) return Unauthorized();

            return Ok(resetpassword.Succeeded);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                                                                "Error: " + ex.Message);
        }
    }

    [HttpDelete("Me"), Authorize]
    public async Task<IActionResult> DeleteMe()
    {
        try
        {
            var userId = this.User.FindFirstValue("id")
                                            ?? throw new Exception("Không tìm thấy User ID");
            var delete = await this.context.DeleteMe(userId);
            if (!delete.Succeeded) return Unauthorized();

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                                                            "Error: " + ex.Message);
        }
    }

}