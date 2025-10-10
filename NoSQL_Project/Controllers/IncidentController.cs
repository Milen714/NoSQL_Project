using ChapeauPOS.Commons;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using NoSQL_Project.Commons;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Services.Interfaces;
using NoSQL_Project.ViewModels;
using System.Reflection;



namespace NoSQL_Project.Controllers
{
	public class IncidentController : BaseController
	{
		private readonly IUserService _userService;
		private readonly ILocationService _locationService;
		private readonly IIncidentService _incidentService;
    public IncidentController(IUserService userService, ILocationService locationSrvice, IIncidentService incidentService, IIncidentService incidents)
        {
            _locationService = locationSrvice;
            _userService = userService;
            _incidentService = incidentService;
            _incidents = incidents;
        }
		public async Task<IActionResult> Index(string searchString, int pageNumber, string currentFilter, string statusFilter, string typeFilter, string branch)
        {
            List<Incident> incidents;

            bool hasStatus = !string.IsNullOrEmpty(statusFilter) && statusFilter != "All";
            bool hasType = !string.IsNullOrEmpty(typeFilter);
			try
			{
				if (searchString != null)
				{
					pageNumber = 1;
				}
				else
				{
					searchString = currentFilter;
				}

				//var incidents = _incidentService.GetAllIncidentsPerStatus(IncidentStatus.open, "").Result;
        
        // Filter by both status and type
              if (hasStatus && hasType &&
                  Enum.TryParse<IncidentStatus>(statusFilter, true, out var parsedStatus) &&
                  Enum.TryParse<IncidentType>(typeFilter, true, out var parsedType))
              {
                  incidents = await _incidentService.GetIncidentsByStatusAndType(parsedStatus, parsedType, branch);
              }
              // Filter by status only
              else if (hasStatus &&
                  Enum.TryParse<IncidentStatus>(statusFilter, true, out parsedStatus))
              {
                  incidents = await _incidentService.GetAllIncidentsPerStatus(parsedStatus, branch);
              }
              // Filter by type only
              else if (hasType &&
                  Enum.TryParse<IncidentType>(typeFilter, true, out parsedType))
              {
                  incidents = await _incidentService.GetAllIncidentsByType(parsedType, branch);
              }
              // No filters — show all
              else
              {
                  incidents = _incidentService.GetAllIncidentsPerStatus(IncidentStatus.open, "").Result;
              }
        
				if (pageNumber < 1)
				{
					pageNumber = 1;
				}

				int pageSize = 10;
				return View(PaginatedList<Incident>.CreateAsync(incidents, pageNumber, pageSize));

			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Could not retrieve Incidents: {ex.Message}";
				return View(new PaginatedList<User>(new List<User>(), 0, 1, 1));
			}
		}

		//Create Incident
		[HttpGet]
		public async Task<IActionResult> CreateIncident()
		{
			var user = HttpContext.Session.GetObject<User>("LoggedInUser");
			if (user == null)
			{
				return Unauthorized();
			}

			var reporterUser = new ReporterSnapshot();
			reporterUser.MapReporter(user);

			var viewModel = new NewIncidentViewModel
			{
				Reporter = reporterUser,

				// Defaults
				Priority = Priority.medium,
				Deadline = 14,
				IncidentType = IncidentType.software,
			};
			ViewBag.Locations = await _locationService.GetAllLocations();

			return View(viewModel);
		}


		//Details 
		public async Task<IActionResult> IncidentDetails(string id, bool isEditing = false)
		{
			try
			{
				Incident incident = await _incidentService.GetIncidentByIdAsync(id);

				if (incident == null)
				{
					TempData["Error"] = "Incident not found.";
					return RedirectToAction("Index");
				}

				ViewBag.IsEditing = isEditing;
				return View(incident);
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Could not retrieve incident details: {ex.Message}";
				return RedirectToAction("Index");
			}
		}

		//Edit Incidents
		[SessionAuthorize(UserType.Service_employee)]
		public IActionResult EditIncident(string id)
		{
			return RedirectToAction("IncidentDetails", new { id, isEditing = true });
		}

		//Update the edited incident
		[HttpPost]
		public async Task<IActionResult> UpdateIncident(Incident updatedIncident)
		{
			foreach (var kvp in ModelState)
			{
				foreach (var error in kvp.Value.Errors)
				{
					Console.WriteLine($"{kvp.Key}: {error.ErrorMessage}");
				}
			}

			if (!ModelState.IsValid)
			{
				ViewBag.IsEditing = true;
				return View("IncidentDetails", updatedIncident);
			}

			try
			{
				await _incidentService.UpdateIncidentAsync(updatedIncident);
			}
			catch (KeyNotFoundException ex)
			{
				TempData["Error"] = ex.Message;
				return RedirectToAction("Index");
			}
			catch (InvalidOperationException ex)
			{
				ModelState.AddModelError("AssignedTo.FirstName", ex.Message);
				ViewBag.IsEditing = true;
				return View("IncidentDetails", updatedIncident);
			}

			return RedirectToAction("IncidentDetails", new { id = updatedIncident.Id });
		}

		[HttpPost]
		public IActionResult ChangePriority(string incidentId, Priority newPriority)
		{
			return RedirectToAction("IncidentDetails", new { id = incidentId });
		}
		[HttpPost]
		public IActionResult ChangeStatus(string incidentId, IncidentStatus status)
		{
			return RedirectToAction("IncidentDetails", new { id = incidentId });
		}
	}
}
