using dixanh.Libraries.Models;

namespace dixanh.Services.Interfaces;

public interface IVehicleStatusHistoryService
{
    Task<List<VehicleStatusHistory>> GetByVehicleAsync(string vehicleId, int take = 200);
}
