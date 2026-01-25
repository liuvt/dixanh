using dixanh.Libraries.Entities;
using dixanh.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace dixanh.Components.Pages.Bases;

public class AppRegisterBase : ComponentBase
{
    [Inject] protected IAuthService AuthService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected ILogger<AppRegisterBase> Logger { get; set; } = default!;

    protected AppRegisterDTO models = new();
    protected bool _processing;
    protected string? textResult;
    protected bool isSuccess;

    // Nếu CheckAuthenState() dùng JS interop => chỉ chạy sau khi interactive
    private bool _checkedAuthOnce;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _checkedAuthOnce) return;
        _checkedAuthOnce = true;

        try
        {
            if (await AuthService.CheckAuthenState())
                Nav.NavigateTo("/", forceLoad: true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "CheckAuthenState failed on register page.");
        }
    }

    // Submit: KHÔNG async void
    protected async Task OnValidSubmit(EditContext editContext)
    {
        _processing = true;
        textResult = null;
        isSuccess = false;
        await InvokeAsync(StateHasChanged);

        try
        {
            var ok = await AuthService.Register(models);
            if (ok)
            {
                isSuccess = true;
                textResult = "Đã đăng ký thành công.";

                // Không block circuit
                await Task.Delay(TimeSpan.FromSeconds(1));

                Nav.NavigateTo("/login", forceLoad: true);
            }
            else
            {
                isSuccess = false;
                textResult = "Đã phát sinh lỗi, vui lòng kiểm tra lại.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Register failed.");
            isSuccess = false;
            textResult = ex.Message; // hoặc message thân thiện hơn
        }
        finally
        {
            _processing = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected Task CleanForm()
    {
        models = new AppRegisterDTO();
        textResult = null;
        isSuccess = false;
        return InvokeAsync(StateHasChanged);
    }

    // Tailwind version: bạn không cần Mud InputType / Icons nữa
    // Chỉ cần bool để toggle type="password"/"text" trong Razor
    protected bool isShowPassword;

    protected void TogglePassword()
    {
        isShowPassword = !isShowPassword;
    }
}
