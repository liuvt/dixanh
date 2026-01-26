using dixanh.Data;
using dixanh.Libraries.Models;
using dixanh.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace dixanh.Services;

public sealed class VehicleStatusHistoryService : IVehicleStatusHistoryService
{
    private readonly dixanhDBContext _db;
    public VehicleStatusHistoryService(dixanhDBContext db) => _db = db;

    // Lấy lịch sử trạng thái xe theo VehicleId
    public Task<List<VehicleStatusHistory>> GetByVehicleAsync(string vehicleId, int take = 200)
        => _db.VehicleStatusHistories.AsNoTracking()
            .Include(x => x.FromStatus)
            .Include(x => x.ToStatus)
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.ChangedAt)
            .Take(take)
            .ToListAsync();
}
