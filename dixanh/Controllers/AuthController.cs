using dixanh.Libraries.Entities;
using dixanh.Libraries.Models;
using dixanh.Servers.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace dixanh.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthServer _auth;
    private readonly UserManager<AppUser> _users;
    private readonly ILogger<AuthController> _logger;
    /*
        // 1) Cookie-only (web nội bộ)
        [Authorize(AuthenticationSchemes = "Identity.Application")]

        // 2) JWT-only (Postman/Mobile)
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        // 3) Cookie OR JWT (dùng policy)
        [Authorize(Policy = "CookieOrJwt")]
     */
    public AuthController(IAuthServer auth, UserManager<AppUser> users, ILogger<AuthController> logger)
    {
        _auth = auth;
        _users = users;
        _logger = logger;
    }

    [HttpPost("Register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] AppRegisterDTO register)
    {
        try
        {
            var result = await _auth.Register(register);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok(true);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error: " + ex.Message);
        }
    }

    // ===================== COOKIE =====================
    // Gọi bằng FORM từ browser: application/x-www-form-urlencoded
    // Sử dụng với mục đích đăng nhập web nội bộ
    [HttpPost("login-cookie")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginCookie([FromForm] AppLoginDTO dto, [FromQuery] string? returnUrl = "/")
    {
        try
        {
            await _auth.LoginCookie(dto);
            return Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        }
        catch (Exception ex)
        {
            // form login thì redirect kèm lỗi cho dễ hiển thị
            return Redirect($"/login?error=1&msg={Uri.EscapeDataString(ex.Message)}");
        }
    }

    [HttpPost("logout-cookie")]
    [Authorize(AuthenticationSchemes = "Identity.Application")]
    public async Task<IActionResult> LogoutCookie([FromQuery] string? returnUrl = "/login")
    {
        await _auth.LogoutCookie();

        // chống open-redirect
        if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
            returnUrl = "/login";

        return LocalRedirect(returnUrl);
    }

    // ===================== JWT =====================
    // Gọi bằng JSON từ SPA/Mobile: application/json
    // Sử dụng với mục đích API token truy cập bằng JWT ra bên ngoài
    [HttpPost("token")]
    [AllowAnonymous]
    public async Task<IActionResult> Token([FromBody] AppLoginDTO dto)
    {
        try
        {
            var token = await _auth.LoginJwt(dto);
            return Ok(token);
        }
        catch (Exception ex)
        {
            return Unauthorized("Error: " + ex.Message);
        }
    }

    // ===================== PROFILE (Cookie + JWT) =====================
    [HttpGet("Me")]
    [Authorize(Policy = "CookieOrJwt")]
    public async Task<ActionResult<AppUser>> GetMe()
    {
        var userId = _users.GetUserId(User); // lấy từ ClaimTypes.NameIdentifier
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized("Not found User ID");

        return Ok(await _auth.GetMe(userId));
    }

    [HttpPatch("Me/Edit")]
    [Authorize(Policy = "CookieOrJwt")]
    public async Task<IActionResult> EditMe([FromBody] AppEditDTO models)
    {
        try
        {
            var userId = _users.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized("Not found User ID");

            var edit = await _auth.EditMe(models, userId);
            if (!edit.Succeeded) return BadRequest(edit.Errors);

            return Ok(true);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error: " + ex.Message);
        }
    }

    [HttpPatch("Me/ChangePassword")]
    [Authorize(Policy = "CookieOrJwt")]
    public async Task<IActionResult> ChangeCurrentPassword([FromBody] AppChangePasswordDTO changePassword)
    {
        try
        {
            var userId = _users.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized("Not found User ID");

            var result = await _auth.ChangeCurrentPassword(userId, changePassword.CurrentPassword, changePassword.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(true);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error: " + ex.Message);
        }
    }

    [HttpDelete("Me")]
    [Authorize(Policy = "CookieOrJwt")]
    public async Task<IActionResult> DeleteMe()
    {
        try
        {
            var userId = _users.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized("Not found User ID");

            var delete = await _auth.DeleteMe(userId);
            if (!delete.Succeeded) return BadRequest(delete.Errors);

            return Ok(true);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error: " + ex.Message);
        }
    }
}
