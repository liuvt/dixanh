
using dixanh.Libraries.Models;
using System.ComponentModel.DataAnnotations;

namespace dixanh.Libraries.Entities;

public class VehicleDto
{
}

// Validation chuẩn theo yêu cầu bạn (Required + IValidatableObject)
public sealed class VehicleCreateDto : IValidatableObject
{
    [Required(ErrorMessage = "Vui lòng chọn Khu vực.")]
    [StringLength(50, ErrorMessage = "Khu vực dài tối đa 50 ký tự.")]
    public string? OperatingArea { get; set; }

    [Required(ErrorMessage = "Tạo mới không được bỏ trống Số hiệu xe.")]
    [StringLength(50, ErrorMessage = "Số hiệu xe dài tối đa 50 ký tự.")]
    public string? CurrentVehicleCode { get; set; }

    [Required(ErrorMessage = "Biển số là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Biển số dài tối đa 50 ký tự.")]
    public string LicensePlate { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Thương hiệu dài tối đa 100 ký tự.")]
    public string? Brand { get; set; }

    [Range(1, 100, ErrorMessage = "Số chỗ phải từ 1 đến 100.")]
    public int? SeatCount { get; set; }

    [StringLength(50, ErrorMessage = "Màu dài tối đa 50 ký tự.")]
    public string? Color { get; set; }

    public DateTimeOffset? ManufactureDate { get; set; }

    [StringLength(100, ErrorMessage = "Loại xe dài tối đa 100 ký tự.")]
    public string? VehicleType { get; set; }

    [StringLength(100, ErrorMessage = "Số khung dài tối đa 100 ký tự.")]
    public string? ChassisNumber { get; set; }

    [StringLength(100, ErrorMessage = "Số máy dài tối đa 100 ký tự.")]
    public string? EngineNumber { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn trạng thái.")]
    public int StatusId { get; set; } = 1;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ManufactureDate.HasValue && ManufactureDate.Value > DateTimeOffset.UtcNow)
        {
            yield return new ValidationResult(
                "Ngày sản xuất không thể lớn hơn ngày hiện tại.",
                new[] { nameof(ManufactureDate) });
        }
    }
}


public sealed class VehicleUpdateDto
{
    public string? CurrentVehicleCode { get; set; }   // số tài
    public string? OperatingArea { get; set; }        // RG/PQ/CT...

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

public sealed class VehicleEditVm
{
    [Required(ErrorMessage = "Biển số là bắt buộc.")]
    public string? LicensePlate { get; set; }

    public string? Brand { get; set; }
    public int? SeatCount { get; set; }
    public string? Color { get; set; }
    public string? VehicleType { get; set; }
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn trạng thái.")]
    public int StatusId { get; set; } = 1;

    public static VehicleEditVm FromModel(Vehicle v) => new()
    {
        LicensePlate = v.LicensePlate,
        Brand = v.Brand,
        SeatCount = v.SeatCount,
        Color = v.Color,
        VehicleType = v.VehicleType,
        ChassisNumber = v.ChassisNumber,
        EngineNumber = v.EngineNumber,
        StatusId = v.StatusId
    };
}
