using MongoDB.Bson;
using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;

namespace NoSQL_Project.Repositories
{
    public class IncidentRepository : IIncidentRepository
    {
        private readonly IMongoCollection<Incident> _incidents;
        public IncidentRepository(IMongoDatabase db)
        {
            _incidents = db.GetCollection<Incident>("INCIDENTS");
        }

        public async Task<List<Incident>> GetAll()
        {
            try
            {
               return await _incidents.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception($"Could not retrieve incidents: {ex.Message}");
            }
        }
        //public async Task<List<Incident>> GetIncidentsByLocation(string locationId)
        //{
        //    try
        //    {
        //        if (!ObjectId.TryParse(locationId, out ObjectId locId))
        //        {
        //            throw new Exception("Invalid locationId format.");
        //        }
        //        var incidents = await _incidents.AsQueryableAsync().Where(i => i.Location.LocationId == locId);
        //        return incidents;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        throw new Exception($"Could not retrieve incidents for location {locationId}: {ex.Message}");
        //    }
        //}
    }
}
