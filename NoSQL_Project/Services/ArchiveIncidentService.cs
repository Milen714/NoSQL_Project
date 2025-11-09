using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Services
{
    public class ArchiveIncidentService : IArchiveIncidentService
    {
        private readonly IArchiveIncidentRepository _archiveIncidentRepository;
        public ArchiveIncidentService(IArchiveIncidentRepository archiveIncidentRepository)
        {
            _archiveIncidentRepository = archiveIncidentRepository;
        }
        public async Task<List<Incident>> GetArchivedIncidents()
        {
            return await _archiveIncidentRepository.GetArchivedIncidents();
        }
        public  async Task ArchiveOldIncidentsAsync(List<Incident> oldIncidents)
        {
            await _archiveIncidentRepository.ArchiveOldIncidentsAsync(oldIncidents);
        }
        public async Task DeleteArchive()
        {
            await _archiveIncidentRepository.DeleteArchive();
        }
    }
}
