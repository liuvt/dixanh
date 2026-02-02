using dixanh.Libraries.Models;
using Microsoft.AspNetCore.Components;

namespace dixanh.Components.Layout;

public static class _TVTTemplate
{
    //Thiết kế UI Tailwind CSS

    // Input
    public static string UiInputPlaceholder => "mt-1 w-full h-10 rounded-lg border border-slate-300 bg-white px-3 text-sm text-slate-900 placeholder-slate-400 focus:border-emerald-500 focus:ring-4 focus:ring-emerald-500/20";
    public static string UiInput => "mt-1 w-full h-10 rounded-lg border border-slate-300 bg-white px-3 text-sm text-slate-900 focus:border-emerald-500 focus:ring-4 focus:ring-emerald-500/20";
    // Select
    public static string UiSelect => "mt-1 w-full h-10 rounded-lg border border-slate-300 bg-white px-3 text-sm text-slate-900 focus:border-emerald-500 focus:ring-4 focus:ring-emerald-500/20";
    // Button
    public static string UiBtnBase => "inline-flex h-10 items-center justify-center rounded-lg px-4 text-sm font-semibold focus:outline-none focus:ring-4 focus:ring-emerald-500/20";
    // Outline button
    public static string UiBtnOutline => $"{UiBtnBase} min-w-[120px] border border-slate-300 bg-white text-slate-700 hover:bg-slate-50";
    // Primary button
    public static string UiBtnPrimary => $"{UiBtnBase} min-w-[120px] border border-emerald-600 bg-emerald-600 text-white hover:border-emerald-700 hover:bg-emerald-700";
    // Small button
    public static string UiBtnSm => "inline-flex h-8 items-center justify-center rounded-md border border-slate-300 bg-white px-3 text-xs font-medium text-slate-700 hover:bg-slate-50";

    // Message box: class div text
    public static string UiMsError => "mb-3 mt-1 rounded-lg border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700";
    public static string UiMsSuccess => "mb-3 mt-1 rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700";
    // Message box: class div reminder
    public static string UiMsReminder => "mt-1 text-xs text-slate-500";

    // Status badge: Trạng thái xe
    public static RenderFragment StatusBadge(string? code, string? name) => builder =>
    {
        var (cls, text) = code switch
        {
            "ACTIVE" => ("border-emerald-200 bg-emerald-50 text-emerald-700", name ?? "Hoạt động"),
            "MAINTENANCE" => ("border-amber-200 bg-amber-50 text-amber-800", name ?? "Bảo trì"),
            "INACTIVE" => ("border-slate-200 bg-slate-50 text-slate-700", name ?? "Ngừng"),
            _ => ("border-slate-200 bg-slate-50 text-slate-700", name ?? "N/A")
        };

        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", $"inline-flex items-center rounded-full border px-2 py-0.5 {cls}");
        builder.AddContent(2, text);
        builder.CloseElement();
    };
}
