using System.Globalization;

namespace dixanh.Helpers;

//1) Extensions cho DateTimeOffset
// Chuẩn hóa lưu trữ và hiển thị theo timezone client
// Lưu ý: DateTimeOffset luôn có timezone kèm theo
// Lưu trong DB luôn là UTC (+00:00)
public static class DateTimeOffsetExtensions
{
    /// <summary>
    /// Chuẩn hóa để LƯU DB: UTC (+00:00)
    /// </summary>
    public static DateTimeOffset ToUtcForStore(this DateTimeOffset value)
        => value.ToUniversalTime();

    public static DateTimeOffset? ToUtcForStore(this DateTimeOffset? value)
        => value?.ToUniversalTime();

    /// <summary>
    /// JS Date.getTimezoneOffset() trả về minutes = (UTC - Local).
    /// Ví dụ VN UTC+7 => -420.
    /// Local offset = -jsOffsetMinutes.
    /// </summary>
    public static DateTimeOffset ToClientOffset(this DateTimeOffset valueUtc, int jsTimezoneOffsetMinutes)
    {
        var clientOffset = TimeSpan.FromMinutes(-jsTimezoneOffsetMinutes);

        // Ép về UTC trước để không lệch nếu lỡ truyền vào không phải UTC
        return valueUtc.ToUniversalTime().ToOffset(clientOffset);
    }

    public static DateTimeOffset? ToClientOffset(this DateTimeOffset? valueUtc, int jsTimezoneOffsetMinutes)
        => valueUtc.HasValue ? valueUtc.Value.ToClientOffset(jsTimezoneOffsetMinutes) : null;

    public static string ToClientString(
        this DateTimeOffset valueUtc,
        int jsTimezoneOffsetMinutes,
        string format = "dd/MM/yyyy HH:mm:ss",
        CultureInfo? culture = null)
    {
        culture ??= CultureInfo.GetCultureInfo("vi-VN");
        return valueUtc.ToClientOffset(jsTimezoneOffsetMinutes).ToString(format, culture);
    }

    public static string ToClientString(
        this DateTimeOffset? valueUtc,
        int jsTimezoneOffsetMinutes,
        string format = "dd/MM/yyyy HH:mm:ss",
        CultureInfo? culture = null)
        => valueUtc.HasValue ? valueUtc.Value.ToClientString(jsTimezoneOffsetMinutes, format, culture) : "";

    /// <summary>
    /// Dùng cho input datetime-local: format bắt buộc "yyyy-MM-ddTHH:mm:ss"
    /// </summary>
    public static string ToDateTimeLocalValue(this DateTimeOffset valueUtc, int jsTimezoneOffsetMinutes)
        => valueUtc.ToClientOffset(jsTimezoneOffsetMinutes)
                   .ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);

    public static string ToDateTimeLocalValue(this DateTimeOffset? valueUtc, int jsTimezoneOffsetMinutes)
        => valueUtc.HasValue ? valueUtc.Value.ToDateTimeLocalValue(jsTimezoneOffsetMinutes) : "";

}

//2) Parse datetime-local (client) -> UTC để lưu
// Lưu ý: datetime-local không có timezone, nên cần offset tại thời điểm đó từ client
// VD: client VN (UTC+7) nhập "2023-08-15T10:30:00", offset = -420
public static class DateTimeLocalExtensions
{
    /// <summary>
    /// Parse giá trị từ input datetime-local (không timezone) thành UTC DateTimeOffset để lưu.
    /// isoLocal: "yyyy-MM-ddTHH:mm:ss" hoặc "yyyy-MM-ddTHH:mm"
    /// jsOffsetMinutesAtThatLocal: Date.getTimezoneOffset() tại thời điểm đó.
    /// </summary>
    public static DateTimeOffset? DateTimeLocalToUtc(this string? isoLocal, int jsOffsetMinutesAtThatLocal)
    {
        if (string.IsNullOrWhiteSpace(isoLocal)) return null;

        var formats = new[]
        {
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm"
        };

        if (!DateTime.TryParseExact(
                isoLocal.Trim(),
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var localUnspecified))
            return null;

        var localOffset = TimeSpan.FromMinutes(-jsOffsetMinutesAtThatLocal);
        var dtoLocal = new DateTimeOffset(localUnspecified, localOffset);

        return dtoLocal.ToUniversalTime();
    }
}

//3) Extension: Parse “dd/MM/yyyy” & “dd/MM/yyyy HH:mm:ss” -> DateTimeOffset (UTC)
// Extensions cho string để parse ngày giờ theo định dạng VN
// Giúp chuẩn hóa việc parse ngày giờ từ string trong code
/* Ví du:
 Dùng trong Seed (gọn lại rất nhiều)
    using dixanh.Extensions;

    ManufactureDate = "26/01/2026".ParseVnDateToUtc(),
    CreatedAt       = "25/01/2026 16:06:03".ParseVnDateTimeToUtc(),
    UpdatedAt       = "25/01/2026 16:06:03".ParseVnDateTimeToUtc(),
 */
public static class StringToDateTimeOffsetExtensions
{
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");
    private static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);

    public static DateTimeOffset ParseVnDateToUtc(this string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentException("Date string is required.", nameof(s));

        var dt = DateTime.ParseExact(s.Trim(), "dd/MM/yyyy", Vi, DateTimeStyles.None);
        return new DateTimeOffset(dt.Year, dt.Month, dt.Day, 0, 0, 0, VnOffset).ToUniversalTime();
    }

    public static DateTimeOffset ParseVnDateTimeToUtc(this string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentException("Datetime string is required.", nameof(s));

        var dt = DateTime.ParseExact(s.Trim(), "dd/MM/yyyy HH:mm:ss", Vi, DateTimeStyles.None);
        return new DateTimeOffset(dt, VnOffset).ToUniversalTime();
    }

    public static bool TryParseVnDateTimeToUtc(this string? s, out DateTimeOffset value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(s)) return false;

        if (!DateTime.TryParseExact(s.Trim(), "dd/MM/yyyy HH:mm:ss", Vi, DateTimeStyles.None, out var dt))
            return false;

        value = new DateTimeOffset(dt, VnOffset).ToUniversalTime();
        return true;
    }
}


//4) Extension: Hiển thị DateTimeOffset (UTC) theo định dạng VN
// Hiển thị ho các trường hợp không cần timezone client động
/*
 Dùng ở Razor:
    @using dixanh.Extensions
    <td>@v.CreatedAt.ToVnDateTime()</td>
 */
public static class DateTimeOffsetDisplayExtensions
{
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");
    private static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);

    public static string ToVnDate(this DateTimeOffset? utc)
        => utc.HasValue ? utc.Value.ToOffset(VnOffset).ToString("dd/MM/yyyy", Vi) : "";

    public static string ToVnDateTime(this DateTimeOffset? utc)
        => utc.HasValue ? utc.Value.ToOffset(VnOffset).ToString("dd/MM/yyyy HH:mm:ss", Vi) : "";
}