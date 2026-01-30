using dixanh.Libraries.Entities;
using dixanh.Libraries.Models;

namespace dixanh.Services.Interfaces;

public interface IVehicleService
{
    Task<(List<Vehicle> Items, int Total)> SearchAsync(
        string? plate,
        int? statusId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int page,
        int pageSize);

    Task<Vehicle?> GetAsync(string vehicleId);

    Task SoftDeleteAsync(string vehicleId, string changedBy, string? reason = null);

    Task RestoreAsync(string vehicleId, string changedBy, string? note = null);

    Task<Vehicle> CreateAsync(VehicleCreateDto dto, string actor);
    Task<Vehicle> UpdateAsync(VehicleUpdateDto dto, string actor);
    Task ChangeStatusAsync(string vehicleId, int toStatusId, string actor, string? note = null);
}
