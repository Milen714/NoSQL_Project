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

		public async Task UpdateBuilderAsync(Incident incident, UpdateDefinition<Incident> update)
		{
			var filter = Builders<Incident>.Filter.Eq(i => i.Id, incident.Id);

			var result = await _incidents.UpdateOneAsync(filter, update);
		}

		public async Task UpdateIncidentAsync(Incident updatedIncident)
		{
			var update = Builders<Incident>.Update
					.Set(i => i.IncidentType, updatedIncident.IncidentType)
					.Set(i => i.Priority, updatedIncident.Priority)
					.Set(i => i.Deadline, updatedIncident.Deadline)
					.Set(i => i.Status, updatedIncident.Status);

			if (updatedIncident.AssignedTo != null)
			{
				update = update
					.Set(i => i.AssignedTo.UserId, updatedIncident.AssignedTo.UserId)
					.Set(i => i.AssignedTo.FirstName, updatedIncident.AssignedTo.FirstName)
					.Set(i => i.AssignedTo.LastName, updatedIncident.AssignedTo.LastName);
			}

			await UpdateBuilderAsync(updatedIncident, update);
		}


		public async Task CloseIncidentAsync(Incident incidentClosed)
		{
			var update = Builders<Incident>.Update
					.Set(i => i.Status, incidentClosed.Status);

			await UpdateBuilderAsync(incidentClosed, update);
		}

		/*
		public async Task CloseIncidentAsync(Incident incidentClosed)
		{
			var filter = Builders<Incident>.Filter.Eq(i => i.Id, incidentClosed.Id);
			var update = Builders<Incident>.Update
					.Set(i => i.Status, incidentClosed.Status);

			var result = await _incidents.UpdateOneAsync(filter, update);
			Console.WriteLine($"Matched: {result.MatchedCount}, Modified: {result.ModifiedCount}");
		}*/
	}
}
