using dixanh.Data;
using dixanh.Libraries.Models;
using dixanh.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace dixanh.Services;

public sealed class VehicleStatusHistoryService : IVehicleStatusHistoryService
{
    private readonly IDbContextFactory<dixanhDBContext> _dbFactory;

    public VehicleStatusHistoryService(IDbContextFactory<dixanhDBContext> dbFactory)
        => _dbFactory = dbFactory;

    // Lấy lịch sử trạng thái xe theo VehicleId
    public async Task<List<VehicleStatusHistory>> GetByVehicleAsync(string vehicleId, int take = 200)
    {
        if (string.IsNullOrWhiteSpace(vehicleId))
            return new List<VehicleStatusHistory>();

        if (take <= 0) take = 200;

        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.VehicleStatusHistories.AsNoTracking()
            .Include(x => x.FromStatus)
            .Include(x => x.ToStatus)
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.ChangedAt)
            .Take(take)
            .ToListAsync();
    }
}
