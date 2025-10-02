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

        public IQueryable<Incident> GetAll()
        {
            try
            {
                IQueryable<Incident> incidents = _incidents.AsQueryable();
                return incidents;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception($"Could not retrieve incidents: {ex.Message}");
            }
        }
    }
}
