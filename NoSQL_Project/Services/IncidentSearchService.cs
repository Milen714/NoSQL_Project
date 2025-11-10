using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Repositories;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Services
{
    public class IncidentSearchService : IIncidentSearchService
    {
        private readonly IIncidentSearchRepository _searchRepository;

        public IncidentSearchService(IIncidentSearchRepository searchRepository) 
        {
            _searchRepository = searchRepository;
        }
        public async Task<List<Incident>> SearchIncidentsAsync(
            string searchTerms,
            SearchOperator searchOperator,
            string branch = "",
            IncidentStatus? status = null,
            IncidentType? type = null)
        {
            
            if (string.IsNullOrWhiteSpace(searchTerms))
            {
                return new List<Incident>();
            }

            return await _searchRepository.SearchIncidentsAsync(searchTerms, searchOperator, branch, status, type);
        }
    }
}