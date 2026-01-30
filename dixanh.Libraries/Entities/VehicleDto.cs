
namespace dixanh.Libraries.Entities;

public class VehicleDto
{
}

public sealed class VehicleCreateDto
{
    public string LicensePlate { get; set; } = "";
    public string Brand { get; set; } = "";
    public int? SeatCount { get; set; }
    public string Color { get; set; } = "";
    public DateTimeOffset? ManufactureDate { get; set; }
    public string VehicleType { get; set; } = "";
    public string ChassisNumber { get; set; } = "";
    public string EngineNumber { get; set; } = "";
    public int StatusId { get; set; }
}

public sealed class VehicleUpdateDto
{
    public string VehicleId { get; set; } = "";
    public string LicensePlate { get; set; } = "";
    public string Brand { get; set; } = "";
    public int? SeatCount { get; set; }
    public string Color { get; set; } = "";
    public DateTimeOffset? ManufactureDate { get; set; }
    public string VehicleType { get; set; } = "";
    public string ChassisNumber { get; set; } = "";
    public string EngineNumber { get; set; } = "";
    public int StatusId { get; set; } // status mới
}
