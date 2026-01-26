using dixanh.Libraries.Models;

namespace dixanh.Services.Interfaces;

public interface IVehicleStatusService
{
    // Lấy tất cả trạng thái xe
    Task<List<VehicleStatus>> GetAllAsync(bool onlyActive = true);
    // Lấy trạng thái xe theo ID
    Task<VehicleStatus?> GetAsync(int id);
    // (Khuyến nghị) Lấy theo Code: ACTIVE/INACTIVE/MAINTENANCE
    // Ví dụ: để lấy trạng thái "Hoạt động", gọi GetByCodeAsync("ACTIVE")
    Task<VehicleStatus?> GetByCodeAsync(string code);
    // Cập nhật trạng thái xe
    Task UpdateAsync(int id, string name, bool isActive, int sortOrder);
}
