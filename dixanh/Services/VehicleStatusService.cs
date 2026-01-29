using dixanh.Data;
using dixanh.Libraries.Models;
using dixanh.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace dixanh.Services;

public sealed class VehicleStatusService : IVehicleStatusService
{
    private readonly IDbContextFactory<dixanhDBContext> _dbFactory;

    public VehicleStatusService(IDbContextFactory<dixanhDBContext> dbFactory)
        => _dbFactory = dbFactory;

    // Lấy tất cả trạng thái xe
    public async Task<List<VehicleStatus>> GetAllAsync(bool onlyActive = true)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var q = db.VehicleStatuses.AsNoTracking();

        if (onlyActive)
            q = q.Where(x => x.IsActive);

        return await q.OrderBy(x => x.SortOrder)
                      .ThenBy(x => x.Name)
                      .ToListAsync();
    }

    // Lấy trạng thái xe theo ID
    public async Task<VehicleStatus?> GetAsync(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.VehicleStatuses.AsNoTracking()
            .FirstOrDefaultAsync(x => x.StatusId == id);
    }

    // Lấy theo Code: ACTIVE/INACTIVE/MAINTENANCE
    public async Task<VehicleStatus?> GetByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        code = code.Trim();

        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.VehicleStatuses.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code);
    }

    // Cập nhật trạng thái xe
    public async Task UpdateAsync(int id, string name, bool isActive, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (sortOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(sortOrder), "SortOrder must be >= 0.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var cur = await db.VehicleStatuses.FirstOrDefaultAsync(x => x.StatusId == id);
        if (cur == null) return;

        // (Tùy chọn) chặn tắt status đang được dùng
        if (cur.IsActive && !isActive)
        {
            var used = await db.Vehicles.AsNoTracking()
                .AnyAsync(v => v.StatusId == id);

            if (used)
                throw new InvalidOperationException("Không thể tắt trạng thái đang được sử dụng bởi xe.");
        }

        cur.Name = name.Trim();
        cur.IsActive = isActive;
        cur.SortOrder = sortOrder;

        await db.SaveChangesAsync();
    }

}
