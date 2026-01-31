using dixanh.Libraries.Entities;

namespace dixanh.Services.Interfaces;

public interface IVehicleCodeHistoryService
{
    Task<List<VehicleCodeHistoryItemDto>> GetByVehicleAsync(string vehicleId, int take = 200);
    Task<VehicleCodeHistoryItemDto?> GetActiveAsync(string vehicleId);
}
