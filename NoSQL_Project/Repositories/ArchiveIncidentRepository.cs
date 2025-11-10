using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;

namespace NoSQL_Project.Repositories
{
    public class ArchiveIncidentRepository : IArchiveIncidentRepository
    {
        private readonly IMongoCollection<Incident> _incidents;
        private readonly IMongoDatabase _db;
        public ArchiveIncidentRepository(ArchiveDatabase archiveDb)
        {
            _db = archiveDb.Database;
            _incidents = _db.GetCollection<Incident>("ARCHIVED_INCIDENTS");
        }
        public async Task<List<Incident>> GetArchivedIncidents()
        {
            return await _incidents.Find(_ => true).ToListAsync();
        }
        public async Task ArchiveOldIncidentsAsync(List<Incident> oldIncidents)
        {
            await _incidents.InsertManyAsync(oldIncidents);
        }

        public async Task DeleteArchive()
        {
            await _incidents.DeleteManyAsync(Builders<Incident>.Filter.Empty);
        }
    }
}
