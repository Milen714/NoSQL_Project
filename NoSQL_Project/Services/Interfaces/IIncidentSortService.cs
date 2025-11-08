using NoSQL_Project.Models;

namespace NoSQL_Project.Services.Interfaces
{
    public interface IIncidentSortService 
    {
        Task<List<Incident>> GetIncidentsSortedByPriorityAsync(string branch = "", bool sortPriorityAscending = false);
    }
}
