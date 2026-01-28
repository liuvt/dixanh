using dixanh.Data;
using dixanh.Libraries.Models;
using dixanh.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace dixanh.Services;

public sealed class VehicleService : IVehicleService
{
    private readonly dixanhDBContext _db;

    // Bạn seed: 1=ACTIVE, 2=INACTIVE, 3=MAINTENANCE
    private const int STATUS_ACTIVE = 1;
    private const int STATUS_INACTIVE = 2;

    public VehicleService(dixanhDBContext db) => _db = db;

    // Lấy thông tin xe theo VehicleId
    public Task<Vehicle?> GetAsync(string vehicleId) =>
        _db.Vehicles.AsNoTracking() 
            .Include(x => x.Status)
            .FirstOrDefaultAsync(x => x.VehicleId == vehicleId);

    // Tìm kiếm xe với phân trang và lọc
    // Trả về tuple (danh sách xe, tổng số bản ghi)
    // plate: lọc theo biển số (chứa chuỗi)
    // statusId: lọc theo trạng thái
    // fromUtc, toUtc: lọc theo khoảng ngày tạo
    // Trả về danh sách xe kèm thông tin trạng thái
    // Sắp xếp theo CreatedAt DESC
    // page: trang hiện tại (bắt đầu từ 1)
    // pageSize: số bản ghi mỗi trang
    // Các filter nên được áp dụng theo thứ tự để tận dụng index
    // Ví dụ gọi: SearchAsync("68G", 1, null, null, 1, 20)
    public async Task<(List<Vehicle> Items, int Total)> SearchAsync(
        string? plate,
        int? statusId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var q = _db.Vehicles.AsNoTracking().AsQueryable();

        // Filter theo biển số (dùng index LicensePlate)
        if (!string.IsNullOrWhiteSpace(plate))
            q = q.Where(x => x.LicensePlate.Contains(plate));

        // Filter theo status (dùng index StatusId, CreatedAt)
        if (statusId.HasValue)
            q = q.Where(x => x.StatusId == statusId.Value);

        // Filter theo ngày tạo (dùng CreatedAt + index combo StatusId, CreatedAt)
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

    // Tạo mới xe và ghi lịch sử trạng thái (mặc định ACTIVE nếu không có).
    // Trả về VehicleId của xe vừa tạo.
    // Nếu muốn tạo xe với trạng thái khác, hãy gọi ChangeStatusAsync sau khi tạo.
    // createdBy: username/mã NV thực hiện thao tác
    public async Task<string> CreateAsync(Vehicle v, string createdBy)
    {
        // Validate tối thiểu
        if (string.IsNullOrWhiteSpace(v.LicensePlate))
            throw new ArgumentException("LicensePlate is required.", nameof(v.LicensePlate));

        if (string.IsNullOrWhiteSpace(v.VehicleId))
            v.VehicleId = Guid.NewGuid().ToString();

        v.CreatedBy = createdBy;
        v.CreatedAt = DateTime.UtcNow;
        v.UpdatedAt = null;

        if (v.StatusId <= 0) v.StatusId = STATUS_ACTIVE;

        // Transaction: tạo xe + ghi history phải đi cùng nhau
        await using var tx = await _db.Database.BeginTransactionAsync();

        _db.Vehicles.Add(v);

        _db.VehicleStatusHistories.Add(new VehicleStatusHistory
        {
            VehicleId = v.VehicleId,
            FromStatusId = null,
            ToStatusId = v.StatusId,
            ChangedAt = v.CreatedAt,
            ChangedBy = createdBy,
            Note = "Create vehicle"
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return v.VehicleId;
    }

    /// <summary>
    /// Chỉ cập nhật thông tin xe - KHÔNG đổi Status ở đây để đúng quy trình.
    /// </summary>
    /// <param name="input">Chứa VehicleId và các thông tin cần cập nhật.</param>
    /// <returns></returns>
    /// </summary>
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
        cur.ManufactureDate = input.ManufactureDate;
        cur.VehicleType = input.VehicleType;
        cur.ChassisNumber = input.ChassisNumber;
        cur.EngineNumber = input.EngineNumber;

        cur.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Đổi trạng thái xe và luôn ghi VehicleStatusHistory.
    /// toStatusId: ID trạng thái đích (phải tồn tại).
    /// </summary>
    public async Task ChangeStatusAsync(string vehicleId, int toStatusId, string changedBy, string? note = null)
    {
        if (string.IsNullOrWhiteSpace(vehicleId))
            throw new ArgumentException("VehicleId is required.", nameof(vehicleId));

        // Optional: check status tồn tại và đang active
        var statusExists = await _db.VehicleStatuses.AsNoTracking()
            .AnyAsync(s => s.StatusId == toStatusId);
        if (!statusExists)
            throw new InvalidOperationException($"StatusId={toStatusId} không tồn tại.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var cur = await _db.Vehicles.FirstOrDefaultAsync(x => x.VehicleId == vehicleId);
        if (cur == null) return;

        if (cur.StatusId == toStatusId) return;

        var from = cur.StatusId;
        cur.StatusId = toStatusId;
        cur.UpdatedAt = DateTime.UtcNow;

        _db.VehicleStatusHistories.Add(new VehicleStatusHistory
        {
            VehicleId = vehicleId,
            FromStatusId = from,
            ToStatusId = toStatusId,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = changedBy,
            Note = note
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    /// <summary>
    /// Không xóa vật lý: chuyển sang INACTIVE.
    /// </summary>
    public Task SoftDeleteAsync(string vehicleId, string changedBy, string? reason = null) =>
        ChangeStatusAsync(vehicleId, STATUS_INACTIVE, changedBy, reason ?? "Soft delete (INACTIVE)");

    public Task RestoreAsync(string vehicleId, string changedBy, string? note = null) =>
        ChangeStatusAsync(vehicleId, STATUS_ACTIVE, changedBy, note ?? "Restore (ACTIVE)");
}
