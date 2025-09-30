using NoSQL_Project.Models;

namespace NoSQL_Project.Services.Interfaces
{
    public interface ILocationService
    {
        Task<List<Location>> GetAllLocations();
    }
}
