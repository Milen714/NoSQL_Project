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
        private readonly IUserService _userService;
		public IncidentService(IIncidentRepository incidentRepository, ILocationService locationService, IUserService userService)
        {
            _incidentRepository = incidentRepository;
            _locationService = locationService;
            _userService = userService;
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

			//Update employee 
			
			var employee = await _userService.FindUserByNameAsync(updatedIncident.AssignedTo.FirstName, updatedIncident.AssignedTo.LastName);
			if (employee == null)
				throw new InvalidOperationException("Employee doesn't exists");

			var assigneeUser = new AssigneeSnapshot();
			assigneeUser.MapAssignee(employee);	        

			existingIncident.AssignedTo = assigneeUser;
			existingIncident.IncidentType = updatedIncident.IncidentType;
			existingIncident.Status = updatedIncident.Status;
			existingIncident.Priority = updatedIncident.Priority;
			existingIncident.Deadline = updatedIncident.Deadline;

			await _incidentRepository.UpdateIncidentAsync(existingIncident);
		}

	}

}
