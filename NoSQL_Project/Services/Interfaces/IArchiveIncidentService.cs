using NoSQL_Project.Models;

namespace NoSQL_Project.Services.Interfaces
{
    public interface IArchiveIncidentService
    {
        Task<List<Incident>> GetArchivedIncidents();
        Task ArchiveOldIncidentsAsync(List<Incident> oldIncidents);
        Task DeleteArchive();
    }
}
