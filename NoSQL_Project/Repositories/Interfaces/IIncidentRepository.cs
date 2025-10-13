using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;

namespace NoSQL_Project.Repositories.Interfaces
{
    public interface IIncidentRepository
    {
        Task<List<Incident>> GetAll();
        Task<List<Incident>> GetAllWitoutclosed(string branch);
        Task<List<Incident>> GetAllIncidentsPerStatus(IncidentStatus status, string branch);

        Task<List<Incident>> GetAllIncidentsByType(IncidentType type, string branch);
        Task<List<Incident>> GetIncidentsByStatusAndType(IncidentStatus status, IncidentType type, string branch);
        Task<Incident> GetIncidentByIdAsync(string id);
        Task <Incident> CreateNewIncidentAsync(Incident newIncident);
        Task<int> GetTheNumberOfAllOpenIncidents();
        Task<int> GetTheNumberOfAllIncidents();

        Task UpdateIncidentAsync(Incident updatedIncident, List<UpdateDefinition<Incident>> updates);


		Task<List<UserForTransferDto>> GetUsersForTransferAsync();

        Task TransferIncidentAsync(Incident existingIncident, User userForTransfer);
        Task<List<Incident>> GetAllOpenOverdueIncidents(string branch);



    }
}
