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
        public async Task<List<Incident>> GetAllWitoutclosed(string branch)
        {
            FilterDefinition<Incident> branchFilter = FilterDefinition<Incident>.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(branch))
                {
                    branchFilter = Builders<Incident>.Filter.Regex("location.branch", new BsonRegularExpression(branch, "i"));
                }
                var excludedStatuses = new[] { IncidentStatus.closed, IncidentStatus.closed_without_resolve };
                var filter = Builders<Incident>.Filter.And(
                    Builders<Incident>.Filter.Nin(i => i.Status, excludedStatuses),
                    branchFilter
                );

                var result = await _incidents.Find(filter).SortBy(p => p.Priority).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {

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
            FilterDefinition < Incident > branchFilter = FilterDefinition<Incident>.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(branch))
                {
                    branchFilter = Builders<Incident>.Filter.Regex("location.branch", new BsonRegularExpression(branch, "i"));
                }
                var excludedStatuses = new[] { IncidentStatus.closed, IncidentStatus.closed_without_resolve };
                var filter = Builders<Incident>.Filter.And(
                    Builders<Incident>.Filter.Eq(i => i.Status, IncidentStatus.resolved),
                    Builders<Incident>.Filter.Nin(i => i.Status, excludedStatuses),
                    branchFilter
                );

                var result = await _incidents.Find(filter).SortBy(p => p.Priority).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {

                throw new Exception($"Could not retrieve incidents: {ex.Message}");
            }
        }

        public async Task<List<Incident>> GetAllIncidentsByType(IncidentType type, string branch)
        {
            FilterDefinition < Incident > branchFilter = FilterDefinition<Incident>.Empty;

            try
            {
                var filter = Builders<Incident>.Filter.And(
                    Builders<Incident>.Filter.Eq(i => i.IncidentType, type), branchFilter);
                if (!string.IsNullOrWhiteSpace(branch))
                    branchFilter = Builders<Incident>.Filter.Regex("location.branch", new BsonRegularExpression(branch, "i"));
                

                var result = await _incidents.Find(filter).SortBy(p => p.Priority).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not retrieve incidents: {ex.Message}");
            }
        }

        public async Task<List<Incident>> GetIncidentsByStatusAndType(IncidentStatus status, IncidentType type, string branch)
        {
            FilterDefinition < Incident > branchFilter = FilterDefinition<Incident>.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(branch))
                    branchFilter = Builders<Incident>.Filter.Regex("location.branch", new BsonRegularExpression(branch, "i"));
                    
                var filter = Builders<Incident>.Filter.And(
                    Builders<Incident>.Filter.Eq(i => i.Status, status),
                    Builders<Incident>.Filter.Eq(i => i.IncidentType, type), 
                    branchFilter);
                   
                return await _incidents.Find(filter).SortBy(p => p.Priority).ToListAsync();
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

		public async Task UpdateIncidentAsync(Incident updatedIncident, List<UpdateDefinition<Incident>> updates)
		{
			var filter = Builders<Incident>.Filter.Eq(i => i.Id, updatedIncident.Id);
			var combinedUpdate = Builders<Incident>.Update.Combine(updates);

			await _incidents.UpdateOneAsync(filter, combinedUpdate);
		}

		public async Task<List<UserForTransferDto>> GetUsersForTransferAsync()
		{
			var pipeline = _incidents.Aggregate()
				.Match(i => i.AssignedTo.IsActive == true)
				.Group(
					i => i.AssignedTo.UserId,
					g => new UserForTransferDto
					{
						UserId = g.Key,
						TotalIncidents = g.Count(),
						FirstName = g.First().AssignedTo.FirstName,
						LastName = g.First().AssignedTo.LastName
					}
				)
				.Match(i => i.TotalIncidents < 6);

			return await pipeline.ToListAsync();

		}
        public async Task<int> GetTheNumberOfAllOpenIncidents()
        {
            try
            {
                BsonDocument stageOne = new BsonDocument
            {
                {
                    "$match", new BsonDocument{
                        {"status", "open" }
                    }
                }
            };

                BsonDocument stageTwo = new BsonDocument
            {
                {
                    "$count" , "open_incident_count"
                }
            };


                BsonDocument[] pipeline = new BsonDocument[] { stageOne, stageTwo };
                var result = await _incidents.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
                if (result != null && result.Contains("open_incident_count"))
                {
                    return result["open_incident_count"].AsInt32;
                }
                return 0;
            }
            catch (Exception ex)
            {

                throw new Exception($"Could not retrieve Number of Open Incidents: {ex.Message}");
            }
        }
        public async Task<int> GetTheNumberOfAllIncidents()
        {
            var filter = Builders<Incident>.Filter.And(
                Builders<Incident>.Filter.Ne(i => i.Status, IncidentStatus.closed),
                Builders<Incident>.Filter.Ne(i => i.Status, IncidentStatus.closed_without_resolve)
                );
            var count = await _incidents.CountDocumentsAsync(filter);
            return (int)count;
        }
    }
}
