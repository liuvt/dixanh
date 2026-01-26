using System.ComponentModel.DataAnnotations;

namespace dixanh.Libraries.Models;

public class Vehicle
{
    // Lưu ý: Nếu MaXe trong DB là INT Identity hoặc UNIQUEIDENTIFIER
    // thì bạn đổi kiểu dữ liệu tương ứng (int / Guid).
    [Key]
    public string VehicleId { get; set; } = Guid.NewGuid().ToString();

    public string LicensePlate { get; set; } = string.Empty;        // Biển kiểm soát (biển số xe). Ví dụ: 68A-12345
    public string VehicleCode { get; set; } = string.Empty;         // Số hiệu xe nội bộ (mã xe trong đội/chi nhánh)
    public string Brand { get; set; } = string.Empty;               // Thương hiệu/Hãng xe. Ví dụ: VinFast, Toyota...
    public int? SeatCount { get; set; }                             // Số chỗ ngồi (4/5/7...), có thể null nếu chưa cập nhật
    public string Color { get; set; } = string.Empty;               // Màu xe (màu sơn). Ví dụ: Trắng, Đen...
    public DateTime? ManufactureDate { get; set; }                  // Ngày sản xuất (hoặc năm sản xuất nếu hệ thống bạn lưu dạng date)
    public string VehicleType { get; set; } = string.Empty;         // Loại xe (ví dụ: Taxi điện, Taxi xăng, 7 chỗ...)
    public string ChassisNumber { get; set; } = string.Empty;       // Số khung (VIN/Chassis Number)
    public string EngineNumber { get; set; } = string.Empty;        // Số máy (Engine Number)

    public string CreatedBy { get; set; } = string.Empty;           // Người tạo bản ghi (username/mã NV)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;      // Thời gian tạo (UTC). Nếu muốn giờ VN: DateTime.Now
    public DateTime? UpdatedAt { get; set; }                        // Thời gian cập nhật gần nhất (UTC), null nếu chưa cập nhật

    // 1-n ICollection thay cho List để linh hoạt hơn trong ORM
    public ICollection<CooperationProfile> CooperationProfiles { get; set; } = new List<CooperationProfile>(); //Hợp đồng hợp tác

}

