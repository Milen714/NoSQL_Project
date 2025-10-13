using MongoDB.Bson;
using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Repositories.Interfaces;

namespace NoSQL_Project.Repositories
{
    public class IncidentRepository : IIncidentRepository
    {
        private readonly IMongoCollection<Incident> _incidents;
        public IncidentRepository(IMongoDatabase db)
        {
            _incidents = db.GetCollection<Incident>("INCIDENTS");
        }

        public async Task<List<Incident>> GetAll()
        {
            try
            {
                return await _incidents.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception($"Could not retrieve incidents: {ex.Message}");
            }
        }
        public async Task<Incident> GetIncidentByIdAsync(string id)
        {
            try
            {
                Incident incident = await _incidents.Find(incident => incident.Id == id).FirstOrDefaultAsync();
                return incident;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception($"Could not retrieve incident {id}: {ex.Message}");
            }
        }
        public async Task<List<Incident>> GetAllIncidentsPerStatus(IncidentStatus status, string branch)
        {

            try
            {
                var filter = Builders<Incident>.Filter.And(
                    Builders<Incident>.Filter.Eq(i => i.Status, status),
                    Builders<Incident>.Filter.Regex("location.branch", new BsonRegularExpression(branch, "i"))
                    );
                var result = await _incidents.Find(filter).SortBy(p => p.Priority).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {

                throw new Exception($"Could not retrieve incidents: {ex.Message}");
            }
        }
        public async Task CreateNewIncidentAsync(Incident newIncident)
        {
            await _incidents.InsertOneAsync(newIncident);
        }

        public async Task<List<Incident>> GetIncidentsByReporter(string reporterId)
        {
            try
            {
                var filter = Builders<Incident>.Filter.Eq("ReportedBy.Id", reporterId);
                return await _incidents.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception($"Could not retrieve incidents for reporter {reporterId}: {ex.Message}");
            }
        }

        public async Task<List<Incident>> SearchIncidentsAsync(
            string searchQuery,
            SearchLogic logic,
            IncidentSort sort,
            string reporterId = null)
        {
            var filters = new List<FilterDefinition<Incident>>();
            var words = searchQuery?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

            if (words.Length > 0)
            {
                var fieldFilters = words.Select(word =>
                    Builders<Incident>.Filter.Or(
                        Builders<Incident>.Filter.Regex(i => i.Subject, new BsonRegularExpression(word, "i")),
                        Builders<Incident>.Filter.Regex(i => i.Description, new BsonRegularExpression(word, "i"))
                ));

                var combinedFilter = logic == SearchLogic.And
                    ? Builders<Incident>.Filter.And(fieldFilters)
                    : Builders<Incident>.Filter.Or(fieldFilters);

                filters.Add(combinedFilter);
            }

            if (!string.IsNullOrEmpty(reporterId))
            {
                filters.Add(Builders<Incident>.Filter.Eq("ReportedBy.Id", reporterId));
            }

            var finalFilter = filters.Count > 0
                ? Builders<Incident>.Filter.And(filters)
                : Builders<Incident>.Filter.Empty;

            SortDefinition<Incident> sortDef = sort switch
            {
                IncidentSort.MostRecent => Builders<Incident>.Sort.Descending(i => i.ReportedAt),
                IncidentSort.PriorityAsc => Builders<Incident>.Sort.Ascending(i => i.Priority),
                IncidentSort.PriorityDesc => Builders<Incident>.Sort.Descending(i => i.Priority),
                _ => Builders<Incident>.Sort.Descending(i => i.ReportedAt)
            };

            return await _incidents.Find(finalFilter).Sort(sortDef).ToListAsync();
        }

    }
}
