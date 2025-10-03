using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Repositories.Incidents;
using NoSQL_Project.Repositories.Locations;
using NoSQL_Project.Services.Locations;
using NoSQL_Project.ViewModels;

namespace NoSQL_Project.Services.Incidents
{
	public class IncidentService : IIncidentService
	{
		private readonly IIncidentRepository _incidentRepository;
		private readonly ILocationService _locationService;


		public IncidentService(IIncidentRepository incidentRepository, ILocationService locationService)
		{
			_incidentRepository = incidentRepository;
			_locationService = locationService;
		}

		public async void CreateNewIncidentAsync(NewIncidentViewModel model)
		{
			string locationBranchName = model.LocationBranchName.ToString();

			//get location snapshot
			LocationSnapshot locationSnapshot = await _locationService.GetLocationSnapshotAsync(locationBranchName);

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
				ReportedAt = DateTime.Now
			};

			//create the incident
			_incidentRepository.CreateNewIncidentAsync(newIncident);

		}

	}
}
