using NoSQL_Project.Models;

namespace NoSQL_Project.Repositories.Interfaces
{
    public interface ILocationRepository
    {
        Task<List<Location>> GetAllLocations();
    }
}
