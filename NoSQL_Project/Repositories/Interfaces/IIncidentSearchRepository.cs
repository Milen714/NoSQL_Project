using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;

namespace NoSQL_Project.Repositories.Interfaces
{
    public interface IIncidentSearchRepository
    {
        Task<List<Incident>> SearchIncidentsAsync(
           string searchTerms,
           SearchOperator searchOperator,
           string branch = "",
           IncidentStatus? status = null,
           IncidentType? type = null);
    }
}

