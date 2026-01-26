using dixanh.Data;
using dixanh.Libraries.Models;
using dixanh.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace dixanh.Services;

public sealed class VehicleStatusService : IVehicleStatusService
{
    private readonly dixanhDBContext _db;
    public VehicleStatusService(dixanhDBContext db) => _db = db;

    // Lấy tất cả trạng thái xe
    public Task<List<VehicleStatus>> GetAllAsync(bool onlyActive = true)
    {
        var q = _db.VehicleStatuses.AsNoTracking();

        if (onlyActive)
            q = q.Where(x => x.IsActive);

        return q.OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .ToListAsync();
    }

    // Lấy trạng thái xe theo ID
    public Task<VehicleStatus?> GetAsync(int id) =>
        _db.VehicleStatuses.AsNoTracking()
           .FirstOrDefaultAsync(x => x.StatusId == id);

    // (Khuyến nghị) Lấy theo Code: ACTIVE/INACTIVE/MAINTENANCE
    // Ví dụ: để lấy trạng thái "Hoạt động", gọi GetByCodeAsync("ACTIVE")
    public Task<VehicleStatus?> GetByCodeAsync(string code) =>
        _db.VehicleStatuses.AsNoTracking()
           .FirstOrDefaultAsync(x => x.Code == code);

    // Cập nhật trạng thái xe
    public async Task UpdateAsync(int id, string name, bool isActive, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (sortOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(sortOrder), "SortOrder must be >= 0.");

        var cur = await _db.VehicleStatuses.FirstOrDefaultAsync(x => x.StatusId == id);
        if (cur == null) return;

        // (Tùy chọn) chặn tắt status đang được dùng
        if (cur.IsActive && !isActive)
        {
            var used = await _db.Vehicles.AsNoTracking().AnyAsync(v => v.StatusId == id);
            if (used)
                throw new InvalidOperationException("Không thể tắt trạng thái đang được sử dụng bởi xe.");
        }

        cur.Name = name; // Cho phép đổi tên
        cur.IsActive = isActive; // Cập nhật trạng thái kích hoạt
        cur.SortOrder = sortOrder; // Cập nhật thứ tự sắp xếp

        await _db.SaveChangesAsync();
    }
}
