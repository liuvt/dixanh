using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dixanh.Libraries.Models;

public class CooperationProfile
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();   // ID bản ghi (GUID string)

    // Vận hành / khai thác
    public string OperatingArea { get; set; } = string.Empty;     // Khu vực hoạt động (khuvuc_hoatdong) - VD: RG/PQ/CT...
    public string OperatingUnit { get; set; } = string.Empty;     // Đơn vị khai thác / công ty thành viên (donvi_khaithac)
    public string ParkingLot { get; set; } = string.Empty;        // Bãi xe/điểm đậu (bai_xe)

    // Trạng thái hợp đồng / kinh doanh
    public string ContractStatus { get; set; } = string.Empty;    // Trạng thái ký hợp đồng (trangthai_ky_hopdong) - VD: Chưa ký/Đã ký/Hủy...
    public string BusinessStatus { get; set; } = string.Empty;    // Trạng thái kinh doanh (trangthai_kinhdoanh) - VD: Đang chạy/Tạm ngưng/Ngừng...

    // Thời gian hợp đồng
    public DateTime? ContractStartDate { get; set; }              // Ngày bắt đầu hợp đồng (ngay_batdau_hopdong)
    public DateTime? ContractEndDate { get; set; }                // Ngày kết thúc hợp đồng (ngay_ketthuc_hopdong)
    public string ContractAttachmentUrl { get; set; } = string.Empty; // File đính kèm hợp đồng (dinhkem_file_hopdong) - link Drive/URL nội bộ

    // Thời gian kinh doanh
    public DateTime? BusinessStartDate { get; set; }              // Ngày bắt đầu kinh doanh (ngay_batdau_kinhdoanh)
    public DateTime? BusinessStopDate { get; set; }               // Ngày ngừng kinh doanh (ngay_ngung_kinhdoanh)
    public string BusinessStopReason { get; set; } = string.Empty;// Lý do ngừng kinh doanh (lydo_ngung_kinhdoanh)

    // Thông tin hợp tác / hình thức
    public string CooperationType { get; set; } = string.Empty;   // Loại hình hợp tác (loaihinh_hoptac) - VD: Thương quyền/Trả góp/Thuê...
    public string BusinessModel { get; set; } = string.Empty;     // Hình thức kinh doanh (hinhthuc_kinhdoanh) - VD: Khoán/Ăn chia/Doanh thu...
    public string CooperationSource { get; set; } = string.Empty; // Nguồn hợp tác (nguon_hoptac) - VD: Giới thiệu, Marketing, Tự đến...

    // Thông tin chủ xe / người thuê (tuỳ loại hợp tác có thể dùng 1 trong các field)
    public string CompanyFranchiseOwnerName { get; set; } = string.Empty; // Họ tên chủ xe thương quyền công ty (hoten_chuxe_thuongquyen_congty)
    public string InstallmentOwnerName { get; set; } = string.Empty;      // Họ tên chủ xe trả góp (hoten_chuxe_tragop)
    public string RenterName { get; set; } = string.Empty;                // Họ tên người thuê (hoten_nguoithue)
    public string PhoneNumber { get; set; } = string.Empty;               // SĐT liên hệ (sđt)

    // Thông tin giấy tờ cá nhân
    public string CitizenIdNumber { get; set; } = string.Empty;   // CCCD/CMND (cccd)
    public DateTime? CitizenIdIssueDate { get; set; }             // Ngày cấp CCCD/CMND (ngay_cap)
    public string CitizenIdIssuePlace { get; set; } = string.Empty;// Nơi cấp CCCD/CMND (noi_cap)
    public string PermanentAddress { get; set; } = string.Empty;  // Địa chỉ hộ khẩu (diachi_hokhau)
    public string ContactAddress { get; set; } = string.Empty;    // Địa chỉ liên lạc (diachi_lienlac)

    // Ghi chú / cập nhật
    public string Notes { get; set; } = string.Empty;             // Ghi chú (ghi_chu)
    public DateTime? UpdatedAt { get; set; }                      // Cập nhật (cap_nhat) - thời điểm cập nhật gần nhất

    public string VehicleId { get; set; } = string.Empty;            // ID liên kết bản ghi chính (Vehicle)
    [ForeignKey("VehicleId")]
    public Vehicle? Vehicle { get; set; } // navigation
}
