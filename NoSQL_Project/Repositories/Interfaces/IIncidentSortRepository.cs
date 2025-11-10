using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;

namespace NoSQL_Project.Repositories.Interfaces
{
    public interface IIncidentSortRepository
    {
        Task<List<Incident>> GetIncidentsSortedByPriorityAsync(string branch = "", bool sortPriorityAscending = false);
    }
}
