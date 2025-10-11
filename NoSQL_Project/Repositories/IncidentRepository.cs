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
			FilterDefinition<Incident> branchFilter = FilterDefinition<Incident>.Empty;
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
			FilterDefinition<Incident> branchFilter = FilterDefinition<Incident>.Empty;

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
			FilterDefinition<Incident> branchFilter = FilterDefinition<Incident>.Empty;
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
			try
			{
				//filter users with active incidents
				BsonDocument stageOne = new BsonDocument
		{
			{ "$match", new BsonDocument { { "assigned_to.is_active", true } } }
		};

				//group by user and count incidents
				BsonDocument stageTwo = new BsonDocument
		{
			{ "$group", new BsonDocument
				{
					{ "_id", "$assigned_to.userId" },
					{ "TotalIncidents", new BsonDocument("$sum", 1) },
					{ "FirstName", new BsonDocument("$first", "$assigned_to.first_name") },
					{ "LastName", new BsonDocument("$first", "$assigned_to.last_name") }
				}
			}
		};

				//filter users with less than 6 incidents
				BsonDocument stageThree = new BsonDocument
		{
			{ "$match", new BsonDocument { { "TotalIncidents", new BsonDocument("$lt", 6) } } }
		};

				
				BsonDocument[] pipeline = new BsonDocument[] { stageOne, stageTwo, stageThree };

				
				var result = await _incidents.Aggregate<BsonDocument>(pipeline).ToListAsync();

				//map to dto
				return result.Select(r => new UserForTransferDto
				{
					UserId = r["_id"].AsObjectId,
					TotalIncidents = r["TotalIncidents"].AsInt32,
					FirstName = r["FirstName"].AsString,
					LastName = r["LastName"].AsString
				}).ToList();
			}
			catch (Exception ex)
			{
				throw new Exception($"Could not retrieve users for transfer: {ex.Message}");
			}
		}


		public async Task TransferIncidentAsync(string incidentId, UserForTransferDto userForTransfer)
		{
			try
			{
				var filter = Builders<Incident>.Filter.Eq(i => i.Id, incidentId);

				var newAssigned = new AssigneeSnapshot
				{
					UserId = userForTransfer.UserId,
					FirstName = userForTransfer.FirstName,
					LastName = userForTransfer.LastName,
					IsActive = true
				};

				//deactivate current active assignee
				var arrayFilters = new List<ArrayFilterDefinition<Incident>>
		{
			new JsonArrayFilterDefinition<Incident>("{ 'elem.is_active': true }")
		};

				var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };
								
				var update = Builders<Incident>.Update.Combine(
					Builders<Incident>.Update.Set("assigned_to.$[elem].is_active", false),
					Builders<Incident>.Update.Push(i => i.AssignedTo, newAssigned)
				);

				await _incidents.UpdateOneAsync(filter, update, updateOptions);

			}
			catch (Exception ex)
			{
				throw new Exception($"Error in transfer {ex.Message}");
			}
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
