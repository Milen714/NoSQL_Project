using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;

namespace NoSQL_Project.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly IMongoCollection<Location> _locations;
        public LocationRepository(IMongoDatabase db)
        {
            _locations = db.GetCollection<Location>("LOCATIONS");
        }
        public async Task<List<Location>> GetAllLocations()
        {
            try
            {
                return await _locations.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<Location> GetLocationById(string id)
        {
            try
            {
                Location location = await _locations.Find(location => location.Id == id).FirstOrDefaultAsync();
                return location;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
