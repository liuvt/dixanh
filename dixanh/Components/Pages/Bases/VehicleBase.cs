using dixanh.Libraries.Entities;
using dixanh.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace dixanh.Components.Pages.Bases;
public class VehicleBase : ComponentBase
{
    [Inject] protected IAuthService AuthService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected ILogger<VehicleBase> Logger { get; set; } = default!;

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
            if (!await AuthService.CheckAuthenState())
                Nav.NavigateTo("/login", forceLoad: true);
        }
        catch (Exception ex)
        {
            // Đừng nuốt exception, ít nhất log để biết nguyên nhân thật
            Logger.LogError(ex, "CheckAuthenState failed on login page.");
        }
    }
  
}