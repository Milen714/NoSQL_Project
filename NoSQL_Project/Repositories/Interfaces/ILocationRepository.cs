using NoSQL_Project.Models;

namespace NoSQL_Project.Repositories.Interfaces
{
    public interface ILocationRepository
    {
        Task<List<Location>> GetAllLocations();
        Task<Location> GetLocationById(string id);

		Task<Location> GetLocationByName(string locationName);
	}
}
