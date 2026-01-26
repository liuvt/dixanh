using dixanh.Libraries.Models;

namespace dixanh.Services.Interfaces;

public interface IVehicleService
{
    Task<(List<Vehicle> Items, int Total)> SearchAsync(
        string? plate, int? statusId, DateTime? fromUtc, DateTime? toUtc, int page, int pageSize);

    Task<Vehicle?> GetAsync(string vehicleId);

    Task<string> CreateAsync(Vehicle v, string createdBy);

    Task UpdateInfoAsync(Vehicle v);

    Task ChangeStatusAsync(string vehicleId, int toStatusId, string changedBy, string? note = null);

    Task SoftDeleteAsync(string vehicleId, string changedBy, string? reason = null);

    Task RestoreAsync(string vehicleId, string changedBy, string? note = null);
}
