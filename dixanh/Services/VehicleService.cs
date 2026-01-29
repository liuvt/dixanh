using dixanh.Data;
using dixanh.Helpers;
using dixanh.Libraries.Models;
using dixanh.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace dixanh.Services;

public sealed class VehicleService : IVehicleService
{
    private readonly IDbContextFactory<dixanhDBContext> _dbFactory;

    private const int STATUS_ACTIVE = 1;
    private const int STATUS_INACTIVE = 2;

    public VehicleService(IDbContextFactory<dixanhDBContext> dbFactory)
        => _dbFactory = dbFactory;

    // Lấy thông tin xe theo ID
    // Ví dụ sử dụng:
    // var vehicle = await vehicleService.GetAsync("vehicle-id-123");
    // if (vehicle != null)
    // {
    //     Console.WriteLine($"{vehicle.LicensePlate} - {vehicle.Brand} - {vehicle.Status?.Name}");
    // }
    // vehicleId: ID của xe cần lấy thông tin
    public async Task<Vehicle?> GetAsync(string vehicleId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Vehicles.AsNoTracking()
            .Include(x => x.Status)
            .FirstOrDefaultAsync(x => x.VehicleId == vehicleId);
    }

    // Tìm kiếm xe với phân trang
    // Ví dụ sử dụng:
    // var (items, total) = await vehicleService.SearchAsync(
    //     plate: "68A",
    //     statusId: 1,
    //     fromUtc: new DateTimeOffset(new DateTime(2023, 1, 1)),
    //     toUtc: new DateTimeOffset(new DateTime(2023, 12, 31)),
    //     page: 1,
    //     pageSize: 20);
    // foreach (var v in items)
    // {
    //     Console.WriteLine($"{v.LicensePlate} - {v.Brand} - {v.Status?.Name}");
    // }
    // Console.WriteLine($"Total vehicles found: {total}");
    // plate: Biển số xe (tìm kiếm chứa)
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

        await using var db = await _dbFactory.CreateDbContextAsync();

        var q = db.Vehicles.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(plate))
            q = q.Where(x => x.LicensePlate.Contains(plate));

        if (statusId.HasValue)
            q = q.Where(x => x.StatusId == statusId.Value);

        if (fromUtc.HasValue)
            q = q.Where(x => x.CreatedAt >= fromUtc.Value);

        if (toUtc.HasValue)
            q = q.Where(x => x.CreatedAt < toUtc.Value);

        var total = await q.CountAsync();

        var items = await q
            .Include(x => x.Status)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    // Tạo mới xe
    // Ví dụ sử dụng:
    // var newVehicleId = await vehicleService.CreateAsync(new Vehicle
    // {
    //     LicensePlate = "68A-12345",
    //     VehicleCode = "TX-001",
    //     Brand = "VinFast",
    //     SeatCount = 5,
    //     Color = "Trắng",
    //     ManufactureDate = new DateTimeOffset(new DateTime(2021, 3, 15)),
    //     VehicleType = "Taxi điện",
    //     ChassisNumber = "RLNV5JSE
    //     EngineNumber = "VFCAFB210
    // }, "admin-user");
    // createdBy: người tạo xe (username/mã NV)
    public async Task<string> CreateAsync(Vehicle v, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(v.LicensePlate))
            throw new ArgumentException("LicensePlate is required.", nameof(v.LicensePlate));

        if (string.IsNullOrWhiteSpace(v.VehicleId))
            v.VehicleId = Guid.NewGuid().ToString();

        await using var db = await _dbFactory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync();

        v.CreatedBy = createdBy;
        v.CreatedAt = DateTimeOffset.UtcNow; // ✅ DateTimeOffset
        v.UpdatedAt = null;

        if (v.StatusId <= 0) v.StatusId = STATUS_ACTIVE;

        db.Vehicles.Add(v);

        db.VehicleStatusHistories.Add(new VehicleStatusHistory
        {
            VehicleId = v.VehicleId,
            FromStatusId = null,
            ToStatusId = v.StatusId,
            ChangedAt = DateTimeOffset.UtcNow, // ✅ DateTimeOffset
            ChangedBy = createdBy,
            Note = "Create vehicle"
        });

        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return v.VehicleId;
    }

    // Cập nhật thông tin xe
    // Ví dụ sử dụng:
    // await vehicleService.UpdateInfoAsync(new Vehicle
    // {
    //     VehicleId = "vehicle-id-123",
    //     LicensePlate = "68A-54321",
    //     VehicleCode = "TX-002",
    //     Brand = "Toyota",
    //     SeatCount = 7,
    //     Color = "Đỏ",
    //     ManufactureDate = new DateTimeOffset(new DateTime(2020, 5, 1)),
    //     VehicleType = "Taxi xăng",
    //     ChassisNumber = "XYZ1234567890",
    //     EngineNumber = "ENG987654
    // });
    public async Task UpdateInfoAsync(Vehicle input)
    {
        if (string.IsNullOrWhiteSpace(input.VehicleId))
            throw new ArgumentException("VehicleId is required.", nameof(input.VehicleId));

        await using var db = await _dbFactory.CreateDbContextAsync();

        var cur = await db.Vehicles.FirstOrDefaultAsync(x => x.VehicleId == input.VehicleId);
        if (cur == null) return;

        cur.LicensePlate = input.LicensePlate;
        cur.VehicleCode = input.VehicleCode;
        cur.Brand = input.Brand;
        cur.SeatCount = input.SeatCount;
        cur.Color = input.Color;
        cur.ManufactureDate = input.ManufactureDate;
        cur.VehicleType = input.VehicleType;
        cur.ChassisNumber = input.ChassisNumber;
        cur.EngineNumber = input.EngineNumber;

        cur.UpdatedAt = DateTimeOffset.UtcNow; // ✅ DateTimeOffset

        await db.SaveChangesAsync();
    }

    // Đổi trạng thái xe và ghi lịch sử
    // toStatusId: 1=ACTIVE, 2=INACTIVE, 3=MAINTENANCE
    // changedBy: người thực hiện thay đổi (username/mã NV)
    // note: ghi chú về thay đổi trạng thái
    // Ví dụ sử dụng:
    // await vehicleService.ChangeStatusAsync("vehicle-id-123", 2, "admin-user", "Ngừng hoạt động do bảo trì");
    public async Task ChangeStatusAsync(string vehicleId, int toStatusId, string changedBy, string? note = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var statusExists = await db.VehicleStatuses.AsNoTracking()
            .AnyAsync(s => s.StatusId == toStatusId);

        if (!statusExists)
            throw new InvalidOperationException($"StatusId={toStatusId} không tồn tại.");

        await using var tx = await db.Database.BeginTransactionAsync();

        var cur = await db.Vehicles.FirstOrDefaultAsync(x => x.VehicleId == vehicleId);
        if (cur == null) return;

        if (cur.StatusId == toStatusId) return;

        var from = cur.StatusId;
        cur.StatusId = toStatusId;
        cur.UpdatedAt = DateTimeOffset.UtcNow;

        db.VehicleStatusHistories.Add(new VehicleStatusHistory
        {
            VehicleId = vehicleId,
            FromStatusId = from,
            ToStatusId = toStatusId,
            ChangedAt = DateTimeOffset.UtcNow,
            ChangedBy = changedBy,
            Note = note
        });

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public Task SoftDeleteAsync(string vehicleId, string changedBy, string? reason = null) =>
        ChangeStatusAsync(vehicleId, STATUS_INACTIVE, changedBy, reason ?? "Soft delete (INACTIVE)");

    public Task RestoreAsync(string vehicleId, string changedBy, string? note = null) =>
        ChangeStatusAsync(vehicleId, STATUS_ACTIVE, changedBy, note ?? "Restore (ACTIVE)");
}
