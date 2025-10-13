using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;

namespace NoSQL_Project.Repositories.Interfaces
{
    public interface IIncidentRepository
    {
        Task<List<Incident>> GetAll();
        Task<List<Incident>> GetAllIncidentsPerStatus(IncidentStatus status, string branch);
        Task<Incident> GetIncidentByIdAsync(string id);
        Task CreateNewIncidentAsync(Incident newIncident);

        Task<List<Incident>> SearchIncidentsAsync(
            string searchQuery,
            SearchLogic logic,
            IncidentSort sort,
            string reporterId = null);
        Task<List<Incident>> GetIncidentsByReporter(string reporterId);
    }
}
