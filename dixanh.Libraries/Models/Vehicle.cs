using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dixanh.Libraries.Models;

public class Vehicle
{
    // Lưu ý: Nếu MaXe trong DB là INT Identity hoặc UNIQUEIDENTIFIER
    // thì bạn đổi kiểu dữ liệu tương ứng (int / Guid).
    [Key]
    [MaxLength(36)]
    public string VehicleId { get; set; } = Guid.NewGuid().ToString();
    [Required, MaxLength(20)]
    public string LicensePlate { get; set; } = string.Empty;        // Biển kiểm soát (biển số xe). Ví dụ: 68A-12345
    [MaxLength(50)]
    public string Brand { get; set; } = string.Empty;               // Thương hiệu/Hãng xe. Ví dụ: VinFast, Toyota...
    public int? SeatCount { get; set; }                             // Số chỗ ngồi (4/5/7...), có thể null nếu chưa cập nhật
    [MaxLength(30)]
    public string Color { get; set; } = string.Empty;               // Màu xe (màu sơn). Ví dụ: Trắng, Đen...
    public DateTimeOffset? ManufactureDate { get; set; }                  // Ngày sản xuất (hoặc năm sản xuất nếu hệ thống bạn lưu dạng date)
    [MaxLength(30)]
    public string VehicleType { get; set; } = string.Empty;         // Loại xe (ví dụ: Taxi điện, Taxi xăng, 7 chỗ...)
    [MaxLength(50)]
    public string ChassisNumber { get; set; } = string.Empty;       // Số khung (VIN/Chassis Number)
    [MaxLength(50)]
    public string EngineNumber { get; set; } = string.Empty;        // Số máy (Engine Number)

    [MaxLength(50)]
    public string CreatedBy { get; set; } = string.Empty;           // Người tạo bản ghi (username/mã NV)
    public DateTimeOffset? CreatedAt { get; set; } = DateTime.UtcNow;      // Thời gian tạo (UTC). Nếu muốn giờ VN: DateTime.Now
    public DateTimeOffset? UpdatedAt { get; set; }                        // Thời gian cập nhật gần nhất (UTC), null nếu chưa cập nhật
    
    public int StatusId { get; set; } // FK đã được cấu hình bằng Fluent API ở dixanhDBContext
    public VehicleStatus? Status { get; set; }     // Nav

    // 1-n ICollection thay cho List để linh hoạt hơn trong ORM
    public ICollection<CooperationProfile> CooperationProfiles { get; set; } = new List<CooperationProfile>(); //Hợp đồng hợp tác
    public ICollection<VehicleStatusHistory> StatusHistories { get; set; } = new List<VehicleStatusHistory>();
}

public class VehicleStatus
{
    [Key]
    public int StatusId { get; set; }   // PK
    [Required, MaxLength(30)]
    public string Code { get; set; } = string.Empty; // Mã trạng thái, ví dụ: ACTIVE, INACTIVE
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty; // Tên trạng thái, ví dụ: Hoạt động, Ngừng hoạt động
    public bool IsActive { get; set; } = true; // Có đang sử dụng trạng thái này không
    public int SortOrder { get; set; } // Thứ tự sắp xếp hiển thị

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>(); // Nav
}

public class VehicleStatusHistory
{
    [Key]
    public long Id { get; set; } // PK
    [Required, MaxLength(36)]
    public string VehicleId { get; set; } = string.Empty; // FK đến Vehicle
    public int? FromStatusId { get; set; } // Trạng thái cũ, có thể null nếu là lần đầu thiết lập
    [Required]
    public int ToStatusId { get; set; } // Trạng thái mới
    public DateTimeOffset? ChangedAt { get; set; } = DateTime.UtcNow; // Thời gian thay đổi trạng thái
    [MaxLength(50)]
    public string? ChangedBy { get; set; } // Người thay đổi trạng thái (username/mã NV)
    [MaxLength(255)]
    public string? Note { get; set; } // Ghi chú về thay đổi trạng thái
    // Optional Navs (khuyến nghị)
    public Vehicle? Vehicle { get; set; } // navigation
    public VehicleStatus? ToStatus { get; set; } // navigation
    public VehicleStatus? FromStatus { get; set; } // navigation
}

