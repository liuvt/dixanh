using dixanh.Libraries.Models;
using dixanh.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace dixanh.Components.Pages.Bases;
public class VehicleBase : ComponentBase
{

    [Inject]
    protected IVehicleService vehicleService { get; set; } = default!;
    [Inject]
    protected IVehicleStatusService vehicleStatusService { get; set; } = default!;

    protected bool _processing;
    protected string? textResult;

    // Cờ để đảm bảo chỉ check auth khi đã interactive (an toàn cho JS interop)

    protected bool _loading;
    protected int _page = 1;
    protected int _pageSize = 20;
    protected int _total;
    protected List<Vehicle> _items = new();
    protected List<VehicleStatus> _statuses = new();

    // filter
    protected string? _filterPlate;
    protected string? _filterVehicleCode;
    protected string? _filterBrand;
    protected int? _filterStatusId;
    protected DateTime? _createdFromLocal;
    protected DateTime? _createdToLocal;

    // drawer state
    protected bool _drawerOpen;
    protected string _drawerMode = "view"; // create|view|edit|history
    protected string? _selectedVehicleId;
    protected string _currentUserName = "system";

    protected override async Task OnInitializedAsync()
    {
        _statuses = await vehicleStatusService.GetAllAsync(onlyActive: true);
        await LoadAsync(page: 1);
    }

    protected async Task LoadAsync(int page)
    {
        _loading = true;
        _page = Math.Max(1, page);

        DateTime? fromUtc = _createdFromLocal?.ToUniversalTime();
        DateTime? toUtc = _createdToLocal?.ToUniversalTime();

        // Nếu bạn muốn filter VehicleCode/Brand ở DB: hãy mở rộng VehicleService.SearchAsync theo request (khuyến nghị),
        // hiện tại demo này gọi theo plate/status/time để đảm bảo chạy ngay.
        var (items, total) = await vehicleService.SearchAsync(
            plate: _filterPlate,
            statusId: _filterStatusId,
            fromUtc: fromUtc,
            toUtc: toUtc,
            page: _page,
            pageSize: _pageSize);

        // filter nhẹ thêm ở client (VehicleCode/Brand) nếu cần tạm thời
        if (!string.IsNullOrWhiteSpace(_filterVehicleCode))
            items = items.Where(x => (x.VehicleCode ?? "").Contains(_filterVehicleCode, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(_filterBrand))
            items = items.Where(x => (x.Brand ?? "").Contains(_filterBrand, StringComparison.OrdinalIgnoreCase)).ToList();

        _items = items;
        _total = total;

        _loading = false;
    }
    protected void ResetFilters()
    {
        _filterPlate = null;
        _filterVehicleCode = null;
        _filterBrand = null;
        _filterStatusId = null;
        _createdFromLocal = null;
        _createdToLocal = null;
    }

    protected void OpenDrawerCreate(string? userName)
    {
        _currentUserName = string.IsNullOrWhiteSpace(userName) ? "system" : userName;
        _drawerMode = "create";
        _selectedVehicleId = null;
        _drawerOpen = true;
    }

    protected void OpenDrawerView(string vehicleId)
    {
        _drawerMode = "view";
        _selectedVehicleId = vehicleId;
        _drawerOpen = true;
    }

    protected void OpenDrawerEdit(string vehicleId)
    {
        _drawerMode = "edit";
        _selectedVehicleId = vehicleId;
        _drawerOpen = true;
    }

    protected void OpenDrawerHistory(string vehicleId)
    {
        _drawerMode = "history";
        _selectedVehicleId = vehicleId;
        _drawerOpen = true;
    }

    protected void CloseDrawer()
    {
        _drawerOpen = false;
        _selectedVehicleId = null;
    }

    protected async Task ReloadAfterSave()
    {
        await LoadAsync(_page);
    }

    /*
    
    [Inject]
    protected IAuthService AuthService { get; set; } = default!;
    [Inject] 
    protected NavigationManager Nav { get; set; } = default!;
    [Inject]
    protected ILogger<VehicleBase> Logger { get; set; } = default!;
    protected bool _checkedAuthOnce;
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
    */
}