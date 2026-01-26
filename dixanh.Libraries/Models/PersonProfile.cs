using System.ComponentModel.DataAnnotations;

namespace dixanh.Libraries.Models
{
    public class PersonProfile
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString(); // ID bản ghi

        public string FullName { get; set; } = string.Empty;        // Họ tên (HO_TEN)
        public string Gender { get; set; } = string.Empty;          // Giới tính (GIOI_TINH) - VD: Nam/Nữ/Khác
        public DateTime? DateOfBirth { get; set; }                  // Ngày sinh (NGAY_SINH)

        // Giấy tờ tùy thân
        public string IdentityDocumentType { get; set; } = string.Empty; // Loại giấy tờ (CMND_CAN_CUOC) - VD: CMND/CCCD/Căn cước
        public string IdentityNumber { get; set; } = string.Empty;       // Số CMND/CCCD (SO CCCD) - lưu string để không mất số 0 đầu
        public DateTime? IdentityIssueDate { get; set; }                 // Ngày cấp (NGAY_CAP)
        public string IdentityIssuePlace { get; set; } = string.Empty;   // Nơi cấp (NOI_CAP)
        public DateTime? IdentityExpiryDate { get; set; }                // Ngày hết hạn CMND/CCCD (NGAY_HET_HAN_CMND)

        // Trình độ / học vấn
        public string QualificationLevel { get; set; } = string.Empty;   // Trình độ bằng cấp (TRINH_DO_BANG_CAP) - VD: Trung cấp/Cao đẳng/ĐH...
        public string EducationLevel { get; set; } = string.Empty;       // Trình độ văn hoá (TRINH_DO_VAN_HOA) - VD: 9/12, 12/12...
        public string Major { get; set; } = string.Empty;                // Chuyên ngành (CHUYEN_NGANH)
        public string MaritalStatus { get; set; } = string.Empty;        // Tình trạng hôn nhân (TINH_TRANG_HON_NHAN) - VD: Độc thân/Đã kết hôn...

        // Liên hệ / địa chỉ
        public string MobilePhone { get; set; } = string.Empty;          // Điện thoại di động (DIEN_THOAI_DI_DONG)
        public string PermanentAddress { get; set; } = string.Empty;     // Địa chỉ thường trú (DIA_CHI_THUONG_TRU)
        public string CurrentAddress { get; set; } = string.Empty;       // Địa chỉ hiện nay (DIA_CHI_HIEN_NAY)
    }
}
