namespace dixanh.Libraries.Entities;

public sealed record PieStatusItemDto(
int StatusId,
string StatusName,
int Count,
double Percent
);

public sealed record TrendPointDto(
    string Label,   // "01/2026" hoặc "2026"
    int Count
);

public sealed class VehicleDashboardDto
{
    public int TotalVehicles { get; set; }
    public DateTimeOffset? LastDataAt { get; set; } // lần đẩy dữ liệu gần nhất
    public List<PieStatusItemDto> PieByStatus { get; set; } = new();
    public List<TrendPointDto> TrendByMonth { get; set; } = new();
    public List<TrendPointDto> TrendByYear { get; set; } = new();
}
