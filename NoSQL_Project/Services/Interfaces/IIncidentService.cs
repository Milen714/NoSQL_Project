using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;

namespace NoSQL_Project.Services.Interfaces
{
    public interface IIncidentService
    {
        List<Incident> GetAll();
        Task<List<Incident>> GetAllIncidentsPerStatus(IncidentStatus status, string branch);
        Task<Incident> GetIncidentByIdAsync(string id);
    }
}
