namespace dixanh.Components.Layout;

public static class _TVTTemplate
{
    //Thiết kế UI Tailwind CSS

    // Input
    public static string UiInput => "mt-1 w-full h-10 rounded-lg border border-slate-300 bg-white px-3 text-sm text-slate-900 placeholder-slate-400 focus:border-emerald-500 focus:ring-4 focus:ring-emerald-500/20";
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
}
