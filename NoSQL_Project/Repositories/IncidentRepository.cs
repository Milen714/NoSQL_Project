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


    }
}
