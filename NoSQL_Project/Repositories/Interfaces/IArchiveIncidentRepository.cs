using NoSQL_Project.Models;

namespace NoSQL_Project.Repositories.Interfaces
{
    public interface IArchiveIncidentRepository
    {
        Task<List<Incident>> GetArchivedIncidents();
        Task ArchiveOldIncidentsAsync(List<Incident> oldIncidents);
        Task DeleteArchive();
    }
}
