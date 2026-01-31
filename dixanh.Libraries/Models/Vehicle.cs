using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dixanh.Libraries.Models;

public class Vehicle
{
    public string VehicleId { get; set; } = Guid.NewGuid().ToString(); // ID bản ghi (GUID string)
    public string CurrentVehicleCode { get; set; } = string.Empty; // Mã xe hiện tại (ma_xe_hientai)

    public string LicensePlate { get; set; } = string.Empty; // Biển số xe (bien_so_xe)
    public string Brand { get; set; } = string.Empty; // Hãng xe (hang_xe)
    public int? SeatCount { get; set; } // Số chỗ ngồi (so_cho_ngoi)
    public string Color { get; set; } = string.Empty; // Màu sắc (mau_sac)
    public DateTimeOffset? ManufactureDate { get; set; } // Ngày sản xuất (ngay_san_xuat)
    public string VehicleType { get; set; } = string.Empty; // Loại xe (loai_xe)
    public string ChassisNumber { get; set; } = string.Empty; // Số khung (so_khung)
    public string EngineNumber { get; set; } = string.Empty; // Số máy (so_may)

    public string CreatedBy { get; set; } = string.Empty; // Người tạo (nguoi_tao)
    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow; // Ngày tạo (ngay_tao)
    public DateTimeOffset? UpdatedAt { get; set; } // Ngày cập nhật (ngay_cap_nhat)

    public int StatusId { get; set; } // Trạng thái xe (trang_thai_xe)
    public VehicleStatus? Status { get; set; } // navigation property

    public ICollection<CooperationProfile> CooperationProfiles { get; set; } = new List<CooperationProfile>(); // navigation property
    public ICollection<VehicleStatusHistory> StatusHistories { get; set; } = new List<VehicleStatusHistory>(); // navigation property
    public ICollection<VehicleCodeHistory> VehicleCodeHistories { get; set; } = new List<VehicleCodeHistory>(); // navigation property
}

public class VehicleStatus
{
    public int StatusId { get; set; } // ID trạng thái (PK)
    public string Code { get; set; } = string.Empty; // Mã trạng thái (VD: ACTIVE, INACTIVE, MAINTENANCE)
    public string Name { get; set; } = string.Empty; // Tên trạng thái
    public bool IsActive { get; set; } = true; // Có sử dụng hay không
    public int SortOrder { get; set; } // Thứ tự sắp xếp

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>(); // navigation property
}

public class VehicleStatusHistory
{
    public long Id { get; set; } // ID bản ghi (PK)
    public string VehicleId { get; set; } = string.Empty; // ID xe (FK)
    public int? FromStatusId { get; set; } // Trạng thái cũ
    public int ToStatusId { get; set; } // Trạng thái mới
    public DateTimeOffset? ChangedAt { get; set; } = DateTimeOffset.UtcNow; // Thời điểm thay đổi
    public string? ChangedBy { get; set; } // Người thay đổi
    public string? Note { get; set; } // Ghi chú

    public Vehicle? Vehicle { get; set; } // navigation property
    public VehicleStatus? ToStatus { get; set; } // navigation property
    public VehicleStatus? FromStatus { get; set; } // navigation property
}

public class VehicleCodeHistory
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); // ID bản ghi (GUID string)

    public string VehicleId { get; set; } = string.Empty; // ID xe (FK)
    public string VehicleCode { get; set; } = string.Empty; // Mã xe (ma_xe)
    public string OperatingArea { get; set; } = string.Empty; // Khu vực hoạt động (khuvuc_hoatdong)

    public DateTimeOffset ValidFrom { get; set; } = DateTimeOffset.UtcNow; // Hiệu lực từ (hieuluc_tu)
    public DateTimeOffset? ValidTo { get; set; } // Hiệu lực đến (hieuluc_den)

    public string? ChangeReason { get; set; } // Lý do thay đổi (lydo_thaydoi)
    public string? ChangedBy { get; set; } // Người thay đổi (nguoi_thaydoi)
    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow; // Thời điểm thay đổi (thoidiem_thaydoi)

    public Vehicle? Vehicle { get; set; } // navigation property
}
