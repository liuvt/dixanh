using dixanh.Data;
using dixanh.Helpers;
using dixanh.Libraries.Models;
using dixanh.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace dixanh.Services;

public sealed class VehicleService : IVehicleService
{
    private readonly dixanhDBContext _db;

    private const int STATUS_ACTIVE = 1;
    private const int STATUS_INACTIVE = 2;

    public VehicleService(dixanhDBContext db) => _db = db;

    public Task<Vehicle?> GetAsync(string vehicleId) =>
        _db.Vehicles.AsNoTracking()
            .Include(x => x.Status)
            .FirstOrDefaultAsync(x => x.VehicleId == vehicleId);

    public async Task<(List<Vehicle> Items, int Total)> SearchAsync(
        string? plate,
        int? statusId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int page,
        int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var q = _db.Vehicles.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(plate))
            q = q.Where(x => x.LicensePlate.Contains(plate)); // hoặc StartsWith để “dễ index” hơn

        if (statusId.HasValue)
            q = q.Where(x => x.StatusId == statusId.Value);

        // ✅ đảm bảo filter là UTC để match DB
        if (fromUtc.HasValue)
        {
            var f = fromUtc.Value.ToUtcForStore();
            q = q.Where(x => x.CreatedAt >= f);
        }

        if (toUtc.HasValue)
        {
            var t = toUtc.Value.ToUtcForStore();
            q = q.Where(x => x.CreatedAt < t);
        }

        var total = await q.CountAsync();

        var items = await q
            .Include(x => x.Status)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<string> CreateAsync(Vehicle v, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(v.LicensePlate))
            throw new ArgumentException("LicensePlate is required.", nameof(v.LicensePlate));

        if (string.IsNullOrWhiteSpace(v.VehicleId))
            v.VehicleId = Guid.NewGuid().ToString();

        var nowUtc = DateTimeOffset.UtcNow; // ✅ chuẩn UTC
        v.CreatedBy = createdBy;
        v.CreatedAt = nowUtc.ToUtcForStore();
        v.UpdatedAt = null;

        if (v.StatusId <= 0) v.StatusId = STATUS_ACTIVE;

        await using var tx = await _db.Database.BeginTransactionAsync();

        _db.Vehicles.Add(v);

        _db.VehicleStatusHistories.Add(new VehicleStatusHistory
        {
            VehicleId = v.VehicleId,
            FromStatusId = null,
            ToStatusId = v.StatusId,
            ChangedAt = nowUtc.ToUtcForStore(), // ✅ DateTimeOffset (khuyến nghị)
            ChangedBy = createdBy,
            Note = "Create vehicle"
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return v.VehicleId;
    }

    public async Task UpdateInfoAsync(Vehicle input)
    {
        if (string.IsNullOrWhiteSpace(input.VehicleId))
            throw new ArgumentException("VehicleId is required.", nameof(input.VehicleId));

        if (string.IsNullOrWhiteSpace(input.LicensePlate))
            throw new ArgumentException("LicensePlate is required.", nameof(input.LicensePlate));

        var cur = await _db.Vehicles.FirstOrDefaultAsync(x => x.VehicleId == input.VehicleId);
        if (cur == null) return;

        cur.LicensePlate = input.LicensePlate;
        cur.VehicleCode = input.VehicleCode;
        cur.Brand = input.Brand;
        cur.SeatCount = input.SeatCount;
        cur.Color = input.Color;
        cur.ManufactureDate = input.ManufactureDate?.ToUtcForStore(); // ✅ nếu bạn muốn luôn UTC
        cur.VehicleType = input.VehicleType;
        cur.ChassisNumber = input.ChassisNumber;
        cur.EngineNumber = input.EngineNumber;

        cur.UpdatedAt = DateTimeOffset.UtcNow.ToUtcForStore(); // ✅

        await _db.SaveChangesAsync();
    }

    public async Task ChangeStatusAsync(string vehicleId, int toStatusId, string changedBy, string? note = null)
    {
        if (string.IsNullOrWhiteSpace(vehicleId))
            throw new ArgumentException("VehicleId is required.", nameof(vehicleId));

        var statusExists = await _db.VehicleStatuses.AsNoTracking()
            .AnyAsync(s => s.StatusId == toStatusId);
        if (!statusExists)
            throw new InvalidOperationException($"StatusId={toStatusId} không tồn tại.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var cur = await _db.Vehicles.FirstOrDefaultAsync(x => x.VehicleId == vehicleId);
        if (cur == null) return;

        if (cur.StatusId == toStatusId) return;

        var nowUtc = DateTimeOffset.UtcNow.ToUtcForStore();
        var from = cur.StatusId;

        cur.StatusId = toStatusId;
        cur.UpdatedAt = nowUtc;

        _db.VehicleStatusHistories.Add(new VehicleStatusHistory
        {
            VehicleId = vehicleId,
            FromStatusId = from,
            ToStatusId = toStatusId,
            ChangedAt = nowUtc,    // ✅
            ChangedBy = changedBy,
            Note = note
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public Task SoftDeleteAsync(string vehicleId, string changedBy, string? reason = null) =>
        ChangeStatusAsync(vehicleId, STATUS_INACTIVE, changedBy, reason ?? "Soft delete (INACTIVE)");

    public Task RestoreAsync(string vehicleId, string changedBy, string? note = null) =>
        ChangeStatusAsync(vehicleId, STATUS_ACTIVE, changedBy, note ?? "Restore (ACTIVE)");
}
