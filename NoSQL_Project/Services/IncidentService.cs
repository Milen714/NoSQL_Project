using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.Services.Interfaces;
using NoSQL_Project.ViewModels;
using System.Collections.Generic;

namespace NoSQL_Project.Services
{
    public class IncidentService : IIncidentService
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly ILocationService _locationService;
        private readonly IUserService _userService;

		private readonly IMongoCollection<Incident> _incidents;

        public IncidentService(IIncidentRepository incidentRepository, ILocationService locationService, IMongoDatabase database, IUserService userService)
        {
            _incidentRepository = incidentRepository;
            _locationService = locationService;
            _incidents = database.GetCollection<Incident>("Incidents");
            _userService = userService;
        }
        public List<Incident> GetAll()
        {
            return _incidentRepository.GetAll().Result;
        }
		public async Task<List<Incident>> GetAllWitoutclosed(string branch)
		{
			return await _incidentRepository.GetAllWitoutclosed(branch);
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
        public async Task<int> GetTheNumberOfAllOpenIncidents()
		{
			return await _incidentRepository.GetTheNumberOfAllOpenIncidents();
		}
		public async Task<int> GetTheNumberOfAllIncidents()
		{
			return await _incidentRepository.GetTheNumberOfAllIncidents();
		}

		public async Task UpdateIncidentAsync(Incident updatedIncident)
		{
			var existingIncident = await _incidentRepository.GetIncidentByIdAsync(updatedIncident.Id);
			if (existingIncident == null)
				throw new KeyNotFoundException("Incident not found");

			var update = Builders<Incident>.Update;
			var updates = new List<UpdateDefinition<Incident>>();

			// Comprobar cambios y agregar a la lista de updates
			if (updatedIncident.IncidentType != existingIncident.IncidentType)
				updates.Add(update.Set(i => i.IncidentType, updatedIncident.IncidentType));

			if (updatedIncident.Priority != existingIncident.Priority)
				updates.Add(update.Set(i => i.Priority, updatedIncident.Priority));

			if (updatedIncident.Deadline != existingIncident.Deadline)
				updates.Add(update.Set(i => i.Deadline, updatedIncident.Deadline));

			if (updatedIncident.Status != existingIncident.Status)
				updates.Add(update.Set(i => i.Status, updatedIncident.Status));

			// Si hay cambios, actualizar
			if (updates.Any())
			{
				await _incidentRepository.UpdateIncidentAsync(updatedIncident, updates);
			}
		}


		/*public async Task<AssigneeSnapshot> BuildAssigneeSnapshotAsync(Incident incident)
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
		}*/

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

			await UpdateIncidentAsync(existingIncident);
		}

		public async Task TransferIncidentAsync(string incidentId, string userForTransferId)
		{
			var existingIncident = await _incidentRepository.GetIncidentByIdAsync(incidentId);
			if (existingIncident == null)
				throw new KeyNotFoundException("Incident not found");

			Console.WriteLine($" {userForTransferId} in service");

			var existingUser = await _userService.FindByIdAsync(userForTransferId);
			if (existingUser == null)
				throw new KeyNotFoundException("User not found");

			//pasar el objeto para que compare el id y el user al que se lo tiene que pasar
			await _incidentRepository.TransferIncidentAsync(incidentId, existingUser);
			
		}

		public async Task <List<UserForTransferDto>> GetUsersForTransferAsync()
		{
			return await _incidentRepository.GetUsersForTransferAsync();			
		}
	}

}
