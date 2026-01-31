using dixanh.Data;
using dixanh.Libraries.Entities;
using dixanh.Libraries.Models;
using dixanh.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace dixanh.Services;

public class VehicleCodeHistoryService : IVehicleCodeHistoryService
{
    private readonly IDbContextFactory<dixanhDBContext> _dbFactory;
    public VehicleCodeHistoryService(IDbContextFactory<dixanhDBContext> dbFactory) => _dbFactory = dbFactory;

    // Lấy lịch sử mã xe của một xe
    // Mặc định lấy 200 bản ghi mới nhất
    // Nếu vehicleId null/empty trả về list rỗng
    public async Task<List<VehicleCodeHistoryItemDto>> GetByVehicleAsync(string vehicleId, int take = 200)
    {
        if (string.IsNullOrWhiteSpace(vehicleId)) return new();
        if (take <= 0) take = 200;

        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.Set<VehicleCodeHistory>()
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.ValidFrom)
            .Take(take)
            .Select(x => new VehicleCodeHistoryItemDto
            {
                Id = x.Id,
                VehicleId = x.VehicleId,
                OperatingArea = x.OperatingArea,
                VehicleCode = x.VehicleCode,
                ValidFrom = x.ValidFrom,
                ValidTo = x.ValidTo,
                ChangedBy = x.ChangedBy,
                ChangedAt = x.ChangedAt,
                ChangeReason = x.ChangeReason
            })
            .ToListAsync();
    }

    // Lấy mã xe hiện tại (active) của xe
    // ValidTo == null
    // Nếu không có trả về null
    // Ví dụ dùng trong tạo chuyến để lấy mã xe gán cho chuyến
    public async Task<VehicleCodeHistoryItemDto?> GetActiveAsync(string vehicleId)
    {
        if (string.IsNullOrWhiteSpace(vehicleId)) return null;

        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.Set<VehicleCodeHistory>()
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId && x.ValidTo == null)
            .OrderByDescending(x => x.ValidFrom)
            .Select(x => new VehicleCodeHistoryItemDto
            {
                Id = x.Id,
                VehicleId = x.VehicleId,
                OperatingArea = x.OperatingArea,
                VehicleCode = x.VehicleCode,
                ValidFrom = x.ValidFrom,
                ValidTo = x.ValidTo,
                ChangedBy = x.ChangedBy,
                ChangedAt = x.ChangedAt,
                ChangeReason = x.ChangeReason
            })
            .FirstOrDefaultAsync();
    }
}
