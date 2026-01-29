using dixanh.Libraries.Entities;
using dixanh.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace dixanh.Components.Pages.Bases;
public class AppLoginBase : ComponentBase
{
    [Inject]
    protected IAuthCookieService AuthService { get; set; } = default!;
    [Inject]
    protected NavigationManager Nav { get; set; } = default!;
    [Inject]
    protected IJSRuntime JS { get; set; } = default!;
    [Inject] protected ILogger<AppLoginBase> Logger { get; set; } = default!;
    protected ElementReference _nativeForm;

    protected AppLoginDTO models = new();
    protected bool _processing;
    protected bool _showPassword;
    protected string? textResult;

    protected override void OnInitialized()
    {
        // nhận msg lỗi từ redirect: /login?msg=...
        var uri = Nav.ToAbsoluteUri(Nav.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);
        if (query.TryGetValue("msg", out var msg))
            textResult = msg.ToString();
    }

    protected void TogglePassword() => _showPassword = !_showPassword;

    protected async Task OnValidSubmit()
    {
        _processing = true;
        // submit native form => browser nhận Set-Cookie Identity
        await JS.InvokeVoidAsync("tvtSubmitForm", _nativeForm);
    }

    protected Task CleanForm()
    {
        models = new AppLoginDTO();
        _processing = false;
        textResult = string.Empty;
        return InvokeAsync(StateHasChanged);
    }

    protected static string InputClass(bool disabled) =>
        "mt-2 block w-full rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-slate-900 shadow-sm outline-none " +
        "focus:border-emerald-500 focus:ring-4 focus:ring-emerald-100 " +
        (disabled ? "opacity-60 cursor-not-allowed" : "");

    protected static string SubmitClass(bool disabled) =>
        "inline-flex items-center justify-center rounded-xl bg-emerald-600 px-4 py-2.5 text-white font-semibold " +
        "shadow-sm hover:bg-emerald-700 focus:outline-none focus:ring-4 focus:ring-emerald-200 " +
        (disabled ? "opacity-70 cursor-not-allowed" : "");
}