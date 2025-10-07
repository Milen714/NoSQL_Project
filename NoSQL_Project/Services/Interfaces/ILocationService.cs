using NoSQL_Project.Models;

namespace NoSQL_Project.Services.Interfaces
{
    public interface ILocationService
    {
        Task<List<Location>> GetAllLocations();
        Task<Location> GetLocationById(string id);
        Task<LocationSnapshot> GetLocationSnapshotAsync(string locationBranchName);
        Task<Location> GetLocationByName(string locationBranchName);
    }
}
