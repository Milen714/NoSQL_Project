using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.Services.Interfaces;
using NoSQL_Project.ViewModels;

namespace NoSQL_Project.Services
{
    public class IncidentService : IIncidentService
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly ILocationService _locationService;

        private readonly IMongoCollection<Incident> _incidents;

        public IncidentService(IIncidentRepository incidentRepository, ILocationService locationService, IMongoDatabase database)
        {
            _incidentRepository = incidentRepository;
            _locationService = locationService;
            _incidents = database.GetCollection<Incident>("Incidents");
        }
        public List<Incident> GetAll()
        {
            return _incidentRepository.GetAll().Result;
        }

        public Task<List<Incident>> GetAllIncidentsPerStatus(IncidentStatus status, string branch)
        {
            return _incidentRepository.GetAllIncidentsPerStatus(status, branch);
        }
        public async Task<Incident> GetIncidentByIdAsync(string id)
        {
            return _incidentRepository.GetIncidentByIdAsync(id).Result;
        }
        
        public Task<List<Incident>> GetAllIncidentsByType(IncidentType type, string branch)
        {
            return _incidentRepository.GetAllIncidentsByType(type, branch);
        }

        public async Task<List<Incident>> GetIncidentsByStatusAndType(IncidentStatus status, IncidentType type, string branch)
        {
            return await _incidentRepository.GetIncidentsByStatusAndType(status, type, branch);
        }


        public async Task CreateNewIncidentAsync(NewIncidentViewModel model)
		{
			//get location branch name
			Location location = await _locationService.GetLocationByName(model.LocationBranchName);

			//get location snapshot
			LocationSnapshot locationSnapshot = new LocationSnapshot();
			locationSnapshot.MapLocationSnapshot(location);

			var newIncident = new Incident
			{
				Subject = model.Subject,
				IncidentType = model.IncidentType,
				Priority = model.Priority,
				Deadline = DateTime.Now.AddDays(model.Deadline),
				Location = locationSnapshot,
				Description = model.Description,
				ReportedBy = model.Reporter,
				Status = IncidentStatus.open,
				ReportedAt = DateTime.UtcNow
			};

			//create the incident
			await _incidentRepository.CreateNewIncidentAsync(newIncident);

		}
        public async Task UpdateIncidentAsync(Incident updatedIncident)
		{
			var existingIncident = await _incidentRepository.GetIncidentByIdAsync(updatedIncident.Id);
			if (existingIncident == null)
				throw new KeyNotFoundException("Incident not found");

			//check what has changed
			var updates = await CheckIncidentUpdates(updatedIncident, existingIncident);

			//if there's anything changed, update it in the existing incident
			if (updates.Any())
			{
				await _incidentRepository.UpdateIncidentAsync(updatedIncident, updates);				
			}
		}

		private async Task <List<UpdateDefinition<Incident>>> CheckIncidentUpdates(Incident updated, Incident existing)
		{
			var update = Builders<Incident>.Update;
			var updates = new List<UpdateDefinition<Incident>>();

			if (updated.IncidentType != existing.IncidentType)
				updates.Add(update.Set(i => i.IncidentType, updated.IncidentType));

			if (updated.Priority != existing.Priority)
				updates.Add(update.Set(i => i.Priority, updated.Priority));

			if (updated.Deadline != existing.Deadline)
				updates.Add(update.Set(i => i.Deadline, updated.Deadline));

			if (updated.Status != existing.Status)
				updates.Add(update.Set(i => i.Status, updated.Status));


			updated.AssignedTo = await BuildAssigneeSnapshotAsync(updated);

			if (existing.AssignedTo == null || updated.AssignedTo.UserId != existing.AssignedTo.UserId)
					updates.Add(update.Set(i => i.AssignedTo.UserId, updated.AssignedTo.UserId));

				if (existing.AssignedTo == null || updated.AssignedTo.FirstName != existing.AssignedTo.FirstName)
					updates.Add(update.Set(i => i.AssignedTo.FirstName, updated.AssignedTo.FirstName));

				if (existing.AssignedTo == null || updated.AssignedTo.LastName != existing.AssignedTo.LastName)
					updates.Add(update.Set(i => i.AssignedTo.LastName, updated.AssignedTo.LastName));
			

			return updates;
		}


		public async Task<AssigneeSnapshot> BuildAssigneeSnapshotAsync(Incident incident)
		{
			if (incident.AssignedTo != null &&
				(!string.IsNullOrWhiteSpace(incident.AssignedTo.FirstName) &&
				!string.IsNullOrWhiteSpace(incident.AssignedTo.LastName)))
			{
				var employee = await _userService.FindUserByNameAsync(incident.AssignedTo.FirstName, incident.AssignedTo.LastName);
				if (employee == null)
					throw new InvalidOperationException("Employee doesn't exists");

				var assigneeUser = new AssigneeSnapshot();
				assigneeUser.MapAssignee(employee);
				incident.AssignedTo = assigneeUser;
			}

			return incident.AssignedTo;
		}


		public async Task CloseIncidentAsync(string closedIncidentId, string updatedStatus)
		{	
			var existingIncident = await _incidentRepository.GetIncidentByIdAsync(closedIncidentId);
			if (existingIncident == null)
				throw new KeyNotFoundException("Incident not found");

			if (IncidentStatus.resolved.ToString().Equals(updatedStatus, StringComparison.OrdinalIgnoreCase))
			{
				existingIncident.Status = IncidentStatus.resolved;
			}
			else if (IncidentStatus.closed_without_resolve.ToString().Equals(updatedStatus, StringComparison.OrdinalIgnoreCase))
			{
				existingIncident.Status = IncidentStatus.closed_without_resolve;
			}
			else
			{
				throw new ArgumentException("Invalid status value");
			}

	}

}
