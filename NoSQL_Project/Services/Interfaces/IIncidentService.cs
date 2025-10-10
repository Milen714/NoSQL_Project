using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.ViewModels;

namespace NoSQL_Project.Services.Interfaces
{
    public interface IIncidentService
    {
        List<Incident> GetAll();
        Task<List<Incident>> GetAllIncidentsPerStatus(IncidentStatus status, string branch);

        Task<List<Incident>> GetAllIncidentsByType(IncidentType type, string branch);
        Task<List<Incident>> GetIncidentsByStatusAndType(IncidentStatus status, IncidentType type, string branch);
        Task<Incident> GetIncidentByIdAsync(string id);
        Task CreateNewIncidentAsync(NewIncidentViewModel model);

        Task UpdateIncidentAsync(Incident updatedIncident);
        Task CloseIncidentAsync(string closedIncidentId, string updatedStatus);
	}
}
