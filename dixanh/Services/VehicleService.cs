using dixanh.Data;
using dixanh.Helpers;
using dixanh.Libraries.Entities;
using dixanh.Libraries.Models;
using dixanh.Services.Interfaces;
using Microsoft.Data.SqlClient;
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
        string? currentVehicleCode,
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
        // filter
        if (!string.IsNullOrWhiteSpace(currentVehicleCode))
            q = q.Where(x => x.CurrentVehicleCode.Contains(currentVehicleCode));
        // filter 
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

    public async Task<Vehicle> CreateAsync(VehicleCreateDto dto, string actor)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync();

        try
        {
            var now = DateTimeOffset.UtcNow;

            // 1) validate StatusId tồn tại
            var statusExists = await db.Set<VehicleStatus>()
                .AnyAsync(x => x.StatusId == dto.StatusId);
            if (!statusExists) throw new InvalidOperationException($"StatusId={dto.StatusId} không tồn tại.");

            // 2) validate biển số unique
            var plate = NormalizePlate(dto.LicensePlate);
            var plateExists = await db.Vehicles.AnyAsync(x => x.LicensePlate == plate);
            if (plateExists) throw new InvalidOperationException($"Biển số '{plate}' đã tồn tại.");

            // 3) normalize số tài + khu vực
            var code = (dto.CurrentVehicleCode ?? "").Trim().ToUpperInvariant();
            var area = (dto.OperatingArea ?? "").Trim().ToUpperInvariant();

            // 4) nếu có nhập số tài => bắt buộc có khu vực
            if (!string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(area))
                throw new InvalidOperationException("Bạn đã nhập Số tài nhưng chưa chọn Khu vực (OperatingArea).");

            // 5) check trùng số tài ACTIVE theo khu vực (để báo lỗi đẹp)
            if (!string.IsNullOrWhiteSpace(code))
            {
                var existsActive = await db.Set<VehicleCodeHistory>().AsNoTracking()
                    .AnyAsync(x => x.OperatingArea == area && x.VehicleCode == code && x.ValidTo == null);

                if (existsActive)
                    throw new InvalidOperationException($"Số tài '{code}' tại khu vực '{area}' đã được dùng bởi xe khác.");
            }

            // 6) create Vehicle
            var vehicle = new Vehicle
            {
                VehicleId = Guid.NewGuid().ToString(),
                CurrentVehicleCode = code, // cache để search nhanh
                LicensePlate = plate,
                Brand = dto.Brand?.Trim() ?? "",
                SeatCount = dto.SeatCount,
                Color = dto.Color?.Trim() ?? "",
                ManufactureDate = dto.ManufactureDate,
                VehicleType = dto.VehicleType?.Trim() ?? "",
                ChassisNumber = dto.ChassisNumber?.Trim() ?? "",
                EngineNumber = dto.EngineNumber?.Trim() ?? "",
                CreatedBy = actor ?? "",
                CreatedAt = now,
                UpdatedAt = null,
                StatusId = dto.StatusId
            };

            db.Vehicles.Add(vehicle);

            // 7) status history INIT
            db.Set<VehicleStatusHistory>().Add(new VehicleStatusHistory
            {
                VehicleId = vehicle.VehicleId,
                FromStatusId = null,
                ToStatusId = dto.StatusId,
                ChangedAt = now,
                ChangedBy = actor,
                Note = "INIT"
            });

            // 8) code history INIT (active) nếu có số tài
            if (!string.IsNullOrWhiteSpace(code))
            {
                db.Set<VehicleCodeHistory>().Add(new VehicleCodeHistory
                {
                    Id = Guid.NewGuid().ToString(),
                    VehicleId = vehicle.VehicleId,
                    OperatingArea = area,
                    VehicleCode = code,
                    ValidFrom = now,
                    ValidTo = null,
                    ChangedAt = now,
                    ChangedBy = actor,
                    ChangeReason = "INIT"
                });
            }

            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return vehicle;
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            await tx.RollbackAsync();

            // trường hợp race-condition: 2 người tạo cùng code/area
            var code = (dto.CurrentVehicleCode ?? "").Trim().ToUpperInvariant();
            var area = (dto.OperatingArea ?? "").Trim().ToUpperInvariant();

            if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(area))
                throw new InvalidOperationException($"Không thể tạo xe: Số tài '{code}' tại khu vực '{area}' đã được dùng.", ex);

            throw;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<Vehicle> UpdateAsync(VehicleUpdateDto dto, string actor)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var vehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.VehicleId == dto.VehicleId);
        if (vehicle is null) throw new KeyNotFoundException($"Không tìm thấy VehicleId={dto.VehicleId}");

        // validate status mới tồn tại
        var statusExists = await db.Set<VehicleStatus>()
            .AnyAsync(x => x.StatusId == dto.StatusId);
        if (!statusExists) throw new InvalidOperationException($"StatusId={dto.StatusId} không tồn tại.");

        // validate biển số unique
        var plate = NormalizePlate(dto.LicensePlate);
        var plateExists = await db.Vehicles.AnyAsync(x => x.VehicleId != dto.VehicleId && x.LicensePlate == plate);
        if (plateExists) throw new InvalidOperationException($"Biển số '{plate}' đã tồn tại.");

        var now = DateTimeOffset.UtcNow;

        // lưu trạng thái cũ để ghi history nếu thay đổi
        var oldStatusId = vehicle.StatusId;

        // cập nhật thông tin (KHÔNG sửa CurrentVehicleCode tại đây)
        vehicle.LicensePlate = plate;
        vehicle.Brand = dto.Brand?.Trim() ?? "";
        vehicle.SeatCount = dto.SeatCount;
        vehicle.Color = dto.Color?.Trim() ?? "";
        vehicle.ManufactureDate = dto.ManufactureDate;
        vehicle.VehicleType = dto.VehicleType?.Trim() ?? "";
        vehicle.ChassisNumber = dto.ChassisNumber?.Trim() ?? "";
        vehicle.EngineNumber = dto.EngineNumber?.Trim() ?? "";
        vehicle.UpdatedAt = now;

        // nếu đổi trạng thái thì ghi lịch sử
        if (oldStatusId != dto.StatusId)
        {
            vehicle.StatusId = dto.StatusId;

            db.Set<VehicleStatusHistory>().Add(new VehicleStatusHistory
            {
                VehicleId = vehicle.VehicleId,
                FromStatusId = oldStatusId,
                ToStatusId = dto.StatusId,
                ChangedAt = now,
                ChangedBy = actor,
                Note = "Update vehicle (status changed)"
            });
        }

        await db.SaveChangesAsync();
        return vehicle;
    }

    // API riêng chỉ đổi trạng thái (đúng nghiệp vụ hơn UpdateAsync nếu chỉ đổi status)
    public async Task ChangeStatusAsync(string vehicleId, int toStatusId, string actor, string? note = null)
    {
        // 1) load vehicle
        await using var db = await _dbFactory.CreateDbContextAsync();

        // 2) validate vehicle tồn tại
        var vehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.VehicleId == vehicleId);
        if (vehicle is null) throw new KeyNotFoundException($"Không tìm thấy VehicleId={vehicleId}");

        // 3) validate status tồn tại
        var statusExists = await db.Set<VehicleStatus>()
            .AnyAsync(x => x.StatusId == toStatusId);
        if (!statusExists) throw new InvalidOperationException($"StatusId={toStatusId} không tồn tại.");

        // 4) nếu khác trạng thái hiện tại thì đổi và ghi lịch sử
        var fromStatusId = vehicle.StatusId;
        if (fromStatusId == toStatusId) return; // không đổi thì thôi

        // 5) đổi trạng thái và ghi lịch sử
        vehicle.StatusId = toStatusId;
        vehicle.UpdatedAt = DateTimeOffset.UtcNow;

        // thêm lịch sử
        db.Set<VehicleStatusHistory>().Add(new VehicleStatusHistory
        {
            VehicleId = vehicle.VehicleId,
            FromStatusId = fromStatusId,
            ToStatusId = toStatusId,
            ChangedAt = DateTimeOffset.UtcNow,
            ChangedBy = actor,
            Note = note
        });
        // 6) lưu
        await db.SaveChangesAsync();
    }

    public Task SoftDeleteAsync(string vehicleId, string changedBy, string? reason = null) =>
        ChangeStatusAsync(vehicleId, STATUS_INACTIVE, changedBy, reason ?? "Soft delete (INACTIVE)");

    public Task RestoreAsync(string vehicleId, string changedBy, string? note = null) =>
        ChangeStatusAsync(vehicleId, STATUS_ACTIVE, changedBy, note ?? "Restore (ACTIVE)");

    private static string NormalizePlate(string s)
    => (s ?? "").Trim().ToUpperInvariant();

    // Thống kê dashboard
    public async Task<VehicleDashboardDto> GetDashboardAsync(int monthsBack = 12, int yearsBack = 5, CancellationToken ct = default)
    {
        if (monthsBack < 1) monthsBack = 12;
        if (yearsBack < 1) yearsBack = 5;

        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        // Total
        var total = await db.Vehicles.AsNoTracking().CountAsync(ct);
        var _lastDataAt = await db.Vehicles.AsNoTracking().MaxAsync(v => (DateTimeOffset?)((v.UpdatedAt ?? v.CreatedAt)));

        // PIE: group theo StatusId, join VehicleStatuses để lấy tên
        var statusCounts = await db.Vehicles.AsNoTracking()
            .GroupBy(v => v.StatusId)
            .Select(g => new { StatusId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var statusNames = await db.VehicleStatuses.AsNoTracking()
            .Select(s => new { s.StatusId, s.Name })
            .ToListAsync(ct);

        var nameMap = statusNames.ToDictionary(x => x.StatusId, x => x.Name);

        var pie = statusCounts
            .Select(x =>
            {
                var name = nameMap.TryGetValue(x.StatusId, out var n) ? n : $"Status {x.StatusId}";
                var percent = total == 0 ? 0 : Math.Round((double)x.Count * 100.0 / total, 2);
                return new PieStatusItemDto(x.StatusId, name, x.Count, percent);
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        // TREND theo CreatedAt (DateTimeOffset?):
        // - loại null CreatedAt (nếu có) hoặc bạn có thể thay null = UtcNow khi seed
        // - group theo Year/Month để SQL translate tốt
        var nowUtc = DateTimeOffset.UtcNow;

        // Trend by Month: monthsBack tháng gần nhất (bao gồm tháng hiện tại)
        var firstMonthUtc = new DateTimeOffset(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var startMonthUtc = firstMonthUtc.AddMonths(-(monthsBack - 1));

        var rawMonth = await db.Vehicles.AsNoTracking()
            .Where(v => v.CreatedAt.HasValue && v.CreatedAt.Value >= startMonthUtc)
            .GroupBy(v => new { v.CreatedAt!.Value.Year, v.CreatedAt!.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(ct);

        var monthMap = rawMonth.ToDictionary(x => (x.Year, x.Month), x => x.Count);

        var trendByMonth = new List<TrendPointDto>(monthsBack);
        for (int i = 0; i < monthsBack; i++)
        {
            var m = startMonthUtc.AddMonths(i);
            var label = $"{m.Month:D2}/{m.Year}";
            trendByMonth.Add(new TrendPointDto(label, monthMap.TryGetValue((m.Year, m.Month), out var c) ? c : 0));
        }

        // Trend by Year: yearsBack năm gần nhất (bao gồm năm hiện tại)
        var startYear = nowUtc.Year - (yearsBack - 1);
        var startYearUtc = new DateTimeOffset(startYear, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var rawYear = await db.Vehicles.AsNoTracking()
            .Where(v => v.CreatedAt.HasValue && v.CreatedAt.Value >= startYearUtc)
            .GroupBy(v => v.CreatedAt!.Value.Year)
            .Select(g => new { Year = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var yearMap = rawYear.ToDictionary(x => x.Year, x => x.Count);

        var trendByYear = new List<TrendPointDto>(yearsBack);
        for (int y = startYear; y <= nowUtc.Year; y++)
            trendByYear.Add(new TrendPointDto(y.ToString(), yearMap.TryGetValue(y, out var c) ? c : 0));

        return new VehicleDashboardDto
        {
            TotalVehicles = total,
            LastDataAt = _lastDataAt,
            PieByStatus = pie,
            TrendByMonth = trendByMonth,
            TrendByYear = trendByYear
        };
    }

    // Đổi mã xe
    // Luồng nghiệp vụ:
    // - Kiểm tra DTO hợp lệ
    // - Bắt đầu transaction
    // - Lấy xe theo VehicleId
    // - Lấy record active hiện tại trong VehicleCodeHistory (nếu có)
    // - Nếu active hiện tại trùng với yêu cầu đổi thì thôi (idempotent)
    // - Nếu có active cũ thì đóng (set ValidTo)
    // - Thêm record mới vào VehicleCodeHistory
    // - Cập nhật CurrentVehicleCode trên Vehicle
    // - Commit transaction
    // - Bắt lỗi unique violation từ DB (nếu có) và trả về lỗi dễ hiểu hơn
    public async Task ChangeVehicleCodeAsync(VehicleCodeChangeDto dto, string actor)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.VehicleId)) throw new ArgumentException("VehicleId rỗng.");
        if (string.IsNullOrWhiteSpace(dto.OperatingArea)) throw new ArgumentException("OperatingArea rỗng.");
        if (string.IsNullOrWhiteSpace(dto.VehicleCode)) throw new ArgumentException("VehicleCode rỗng.");

        var vehicleId = dto.VehicleId.Trim();
        var area = dto.OperatingArea.Trim().ToUpperInvariant();
        var code = dto.VehicleCode.Trim().ToUpperInvariant();
        var now = DateTimeOffset.UtcNow;

        await using var db = await _dbFactory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync();

        try
        {
            var vehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.VehicleId == vehicleId);
            if (vehicle is null) throw new KeyNotFoundException($"Không tìm thấy VehicleId={vehicleId}");

            // Active record hiện tại (nếu có)
            var active = await db.Set<VehicleCodeHistory>()
                .Where(x => x.VehicleId == vehicleId && x.ValidTo == null)
                .OrderByDescending(x => x.ValidFrom)
                .FirstOrDefaultAsync();

            // Nếu đang active đúng y như vậy thì thôi (idempotent)
            if (active != null
                && string.Equals(active.OperatingArea, area, StringComparison.OrdinalIgnoreCase)
                && string.Equals(active.VehicleCode, code, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Đóng active cũ
            if (active != null)
            {
                active.ValidTo = now;
                active.ChangedAt = now;
                active.ChangedBy = actor;
                active.ChangeReason = string.IsNullOrWhiteSpace(dto.ChangeReason) ? "Đổi số hiệu" : dto.ChangeReason;
            }

            // Insert active mới
            db.Set<VehicleCodeHistory>().Add(new VehicleCodeHistory
            {
                Id = Guid.NewGuid().ToString(),
                VehicleId = vehicleId,
                OperatingArea = area,
                VehicleCode = code,
                ValidFrom = now,
                ValidTo = null,
                ChangedAt = now,
                ChangedBy = actor,
                ChangeReason = dto.ChangeReason
            });

            // Update số hiệu hiện tại trên Vehicle
            vehicle.CurrentVehicleCode = code;
            vehicle.UpdatedAt = now;

            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            await tx.RollbackAsync();
            // filtered unique index sẽ đẩy lỗi khi:
            // - 1 xe có 2 active
            // - 1 khu vực trùng code active với xe khác
            throw new InvalidOperationException($"Không thể đổi số hiệu: Số hiệu '{code}' tại khu vực '{area}' đang được dùng bởi xe khác (hoặc xe này đang có 2 số hiệu active).", ex);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // Kiểm tra lỗi unique constraint/index từ DbUpdateException
    // Sử dụng hàm này để bắt lỗi unique và trả về lỗi nghiệp vụ dễ hiểu hơn
    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        // SQL Server unique constraint/index: 2601, 2627
        var sqlEx = ex.InnerException as SqlException
                    ?? ex.GetBaseException() as SqlException;

        if (sqlEx == null) return false;
        return sqlEx.Number is 2601 or 2627;
    }
}
