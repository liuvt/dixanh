using dixanh.Libraries.Entities;
using dixanh.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace dixanh.Components.Pages.Bases;
public class AppLoginBase : ComponentBase
{
    [Inject] protected IAuthService AuthService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected ILogger<AppLoginBase> Logger { get; set; } = default!;

    protected AppLoginDTO models = new();
    protected bool _processing;
    protected string? textResult;

    // Cờ để đảm bảo chỉ check auth khi đã interactive (an toàn cho JS interop)
    private bool _checkedAuthOnce;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _checkedAuthOnce) return;
        _checkedAuthOnce = true;

        try
        {
            // Nếu CheckAuthenState() có JS interop (localStorage), gọi ở đây là đúng chỗ
            if (await AuthService.CheckAuthenState())
                Nav.NavigateTo("/dashboard", forceLoad: true);
        }
        catch (Exception ex)
        {
            // Đừng nuốt exception, ít nhất log để biết nguyên nhân thật
            Logger.LogError(ex, "CheckAuthenState failed on login page.");
        }
    }

    // Submit: TUYỆT ĐỐI không dùng async void
    protected async Task OnValidSubmit(EditContext editContext)
    {
        _processing = true;
        textResult = null;
        await InvokeAsync(StateHasChanged);

        try
        {
            await AuthService.Login(models);

            // Không dùng Thread.Sleep trong Blazor Server
            await Task.Delay(TimeSpan.FromSeconds(1));

            Nav.NavigateTo("/dashboard", forceLoad: true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Login failed.");
            textResult = "Đăng nhập thất bại. Vui lòng kiểm tra email/mật khẩu hoặc thử lại.";
            await CleanForm();
        }
        finally
        {
            _processing = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected Task CleanForm()
    {
        models = new AppLoginDTO();
        _processing = false;
        return InvokeAsync(StateHasChanged);
    }
}