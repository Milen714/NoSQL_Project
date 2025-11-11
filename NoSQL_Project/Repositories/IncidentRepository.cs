using MongoDB.Bson;
using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Repositories.Interfaces;
using Sprache;

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
                var excludedStatuses = new[] { IncidentStatus.closed, IncidentStatus.closed_without_resolve, IncidentStatus.resolved };
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
                throw new Exception($"Could not retrieve incident {id}: {ex.Message}");
            }
        }
        public async Task<List<Incident>> GetAllIncidentsPerStatus(IncidentStatus status, string branch)
        {
            try
            {
                // Start aggregation pipeline
                var pipeline = _incidents.Aggregate();

                // A branch filter
                if (!string.IsNullOrWhiteSpace(branch))
                {
                    var regex = new BsonRegularExpression(branch, "i");
                    pipeline = pipeline.Match(Builders<Incident>.Filter.Regex("location.branch", regex));
                }

                // Handle closed statuses
                if (status == IncidentStatus.closed || status == IncidentStatus.closed_without_resolve)
                {
                    pipeline = pipeline
                        .Match(i => i.Status == status)
                        .SortBy(i => i.Priority);

                    return await pipeline.ToListAsync();
                }

                // Otherwise exclude closed statuses
                var excludedStatuses = new[] { IncidentStatus.closed, IncidentStatus.closed_without_resolve };

                pipeline = pipeline
                    .Match(i => i.Status == status && !excludedStatuses.Contains(i.Status))
                    .SortBy(i => i.Priority);


                var result = await pipeline.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not retrieve incidents: {ex.Message}");
            }
        }

        public async Task<List<Incident>> GetAllIncidentsByType(IncidentType type, string branch)
        {
            try
            {
                var pipeline = new List<BsonDocument>();


                // Match by type
                var matchStage = new BsonDocument("$match", new BsonDocument("incident_type", type.ToString()));
                // match by type first
                pipeline.Add(matchStage);

                // If branch is specified, add regex filter on location.branch
                if (!string.IsNullOrWhiteSpace(branch))
                {
                    var branchMatch = new BsonDocument("$match", new BsonDocument("location.branch",
                        new BsonDocument("$regex", branch).Add("$options", "i")));
                    pipeline.Add(branchMatch);
                }
                // Sort by ascending
                var sortStage = new BsonDocument("$sort", new BsonDocument("priority", 1));
                pipeline.Add(sortStage);

                var result = await _incidents.Aggregate<Incident>(pipeline).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not retrieve incidents: {ex.Message}");
            }
        }

        public async Task<List<Incident>> GetIncidentsByStatusAndType(IncidentStatus status, IncidentType type, string branch)
        {
            try
            {
                var pipeline = _incidents.Aggregate()
                    .Match(i => i.Status == status && i.IncidentType == type);


                if (!string.IsNullOrWhiteSpace(branch))
                {
                    var regex = new BsonRegularExpression(branch, "i");
                    pipeline = pipeline.Match(Builders<Incident>.Filter.Regex("location.branch", regex));
                }


                pipeline = pipeline.SortBy(i => i.Priority);

                var result = await pipeline.ToListAsync();
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

		public async Task UpdateIncidentAsync(Incident updatedIncident, List<UpdateDefinition<Incident>> updates)
		{
			try
			{
				//to find the incident to make the changes 
				var filter = Builders<Incident>.Filter.Eq(i => i.Id, updatedIncident.Id);

				//to combine the elements of updates list into a single update
				var combinedUpdate = Builders<Incident>.Update.Combine(updates);

				//filter: documents to update, combinedUpdate: changes to make
				await _incidents.UpdateOneAsync(filter, combinedUpdate);
			}
			catch(Exception ex)
			{
				throw new Exception("Could not update incident", ex);
			}
		}

		public async Task<List<UserForTransferDto>> GetUsersForTransferAsync()
		{
			try
			{
				var pipeline = new List<BsonDocument>();

				//stage 1: unwind the array assigned_to
				pipeline.Add(new BsonDocument("$unwind", "$assigned_to"));

				//stage 2: filter users who have active incidents
				pipeline.Add(new BsonDocument("$match", new BsonDocument("assigned_to.is_active", true)));

				//stage 3: group by user and count their incidents
				pipeline.Add(new BsonDocument("$group", new BsonDocument
				{
					{"_id", "$assigned_to.userId"},
					{"TotalIncidents", new BsonDocument("$sum", 1)},
					{"FirstName", new BsonDocument("$first", "$assigned_to.first_name")},
					{"LastName", new BsonDocument("$first", "$assigned_to.last_name")}
				}));

				//stage 4: filter by users with less than 6 incidents
				pipeline.Add(new BsonDocument("$match", new BsonDocument("TotalIncidents", new BsonDocument("$lt", 6))));

				//stage 5: sort users alphabetically by First Name and then Last Name
				pipeline.Add(new BsonDocument("$sort", new BsonDocument
				{
					{"FirstName", 1},
					{"LastName", 1}
				}));

				var result = await _incidents.Aggregate<BsonDocument>(pipeline).ToListAsync();

				return result.Select(r => new UserForTransferDto
				{
					UserId = r.GetValue("_id", ObjectId.Empty).AsObjectId,
					TotalIncidents = r.GetValue("TotalIncidents", 0).AsInt32,
					FirstName = r.GetValue("FirstName", "(Unknown)").AsString,
					LastName = r.GetValue("LastName", "").AsString
				}).ToList();
			}
			catch (Exception ex)
			{
				throw new Exception("Could not retrieve users for transfer", ex);
			}
		}

		public async Task TransferIncidentAsync(Incident existingIncident, User userForTransfer)
		{
			try
			{
				var filter = Builders<Incident>.Filter.Eq("_id", ObjectId.Parse(existingIncident.Id));

				if (existingIncident.AssignedTo != null && existingIncident.AssignedTo.Any())
				//only deactivate if the array is not empty
				{
					var deactivate = Builders<Incident>.Update
					.Set("assigned_to.$[elem].is_active", false);

                    var arrayFilters = new List<ArrayFilterDefinition>
                    {
                        new JsonArrayFilterDefinition<BsonDocument>("{ 'elem.is_active': true }")
                    };

                    var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };

					await _incidents.UpdateOneAsync(filter, deactivate, updateOptions);
				}

				var newAssignee = new AssigneeSnapshot
				{
					UserId = new ObjectId(userForTransfer.Id),
					FirstName = userForTransfer.FirstName,
					LastName = userForTransfer.LastName,
					IsActive = true,
					EmailAddress = userForTransfer.EmailAddress,
					Timestamp = DateTime.UtcNow
				};

                var addNew = Builders<Incident>.Update.Push(i => i.AssignedTo, newAssignee);

				await _incidents.UpdateOneAsync(filter, addNew);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in transfer: ", ex);
			}
		}

		public async Task AddTransferMessage(Incident existingIncident, AssigneeSnapshot userBeforeTransfer, string transferMessage)
		{
			try
			{
				var filter = Builders<Incident>.Filter.And(
					Builders<Incident>.Filter.Eq(i => i.Id, existingIncident.Id),
					Builders<Incident>.Filter.Eq("assigned_to.userId", userBeforeTransfer.UserId)
				);

				var update = Builders<Incident>.Update.Set("assigned_to.$.message", transferMessage);

				await _incidents.UpdateOneAsync(filter, update);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in adding transfer message: ", ex);
			}
		}

		public async Task<List<AssigneeSnapshot>> GetTransferHistory(Incident existingIncident)
		{
			try
			{
				var filter = Builders<Incident>.Filter.Eq(i => i.Id, existingIncident.Id);

				var incident = await _incidents.Find(filter).FirstOrDefaultAsync();

				//if assinged to is null for any reason, it returns an empty list 
				if (incident == null || incident.AssignedTo == null)
				{
					return new List<AssigneeSnapshot>();
				}
				return incident.AssignedTo;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error getting assignation history: ", ex);
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
        public async Task<List<Incident>> GetAllOpenOverdueIncidents(string branch)
        {
            FilterDefinition<Incident> branchFilter = FilterDefinition<Incident>.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(branch))
                {
                    branchFilter = Builders<Incident>.Filter.Regex("location.branch", new BsonRegularExpression(branch, "i"));
                }

                var filter = Builders<Incident>.Filter.And(
                    Builders<Incident>.Filter.Lt(i => i.Deadline, DateTime.UtcNow.AddHours(8)),
                    Builders<Incident>.Filter.In(i => i.Status, new[] { IncidentStatus.open, IncidentStatus.inProgress }),
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
        public async Task<List<Incident>> GetAwaitingToBeArchivedIncidents()
        {
            var twoYearsAgo = DateTime.UtcNow.AddYears(-2);
            var filter = Builders<Incident>.Filter.Lt(i => i.ReportedAt, twoYearsAgo);
            var result = await _incidents.Find(filter).ToListAsync();
            return result;
        }
        public async Task DeleteArchivedIncidents(List<Incident> toDelete)
        {
            try
            {
                List<ObjectId> toDeleteIds = new List<ObjectId>();
                foreach (Incident incident in toDelete)
                {
                    if (!string.IsNullOrWhiteSpace(incident.Id))
                    {
                        toDeleteIds.Add(ObjectId.Parse(incident.Id));
                    }
                }
                var filter = Builders<Incident>.Filter.In("_id", toDeleteIds);
                await _incidents.DeleteManyAsync(filter);
            }
            catch (Exception ex)
            {
                throw new Exception($"Something went wrong while deleting the incidents to be archived: {ex.Message}");
            }
        }

    }


}
