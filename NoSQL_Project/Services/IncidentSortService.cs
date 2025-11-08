using NoSQL_Project.Models;
using NoSQL_Project.Repositories;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Services
{
    public class IncidentSortService : IIncidentSortService
    {
        private readonly IIncidentSortRepository _sortRepository; 

        public IncidentSortService(IIncidentSortRepository sortRepository) 
        {
            _sortRepository = sortRepository;
        }

        public async Task<List<Incident>> GetIncidentsSortedByPriorityAsync(string branch = "", bool sortPriorityAscending = false)
        {
            return await _sortRepository.GetIncidentsSortedByPriorityAsync(branch, sortPriorityAscending);
        }
    }
}
