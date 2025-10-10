using ChapeauPOS.Commons;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using NoSQL_Project.Commons;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Services.Interfaces;
using NoSQL_Project.ViewModels;

namespace NoSQL_Project.Controllers
{
	public class IncidentController : BaseController
	{
		private readonly IUserService _userService;
		private readonly ILocationService _locationService;
		private readonly IIncidentService _incidentService;
		public IncidentController(IUserService userService, ILocationService locationSrvice, IIncidentService incidentService)
		{
			_locationService = locationSrvice;
			_userService = userService;
			_incidentService = incidentService;
		}
		public async Task<IActionResult> Index(string searchString, int pageNumber, string currentFilter)
		{
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

				var incidents = _incidentService.GetAllIncidentsPerStatus(IncidentStatus.open, "").Result;

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

		[SessionAuthorize(UserType.Service_employee)]
		[HttpPost]
		public async Task<IActionResult> CloseIncident (string incidentId, string updatedStatus)
		{
			try
			{
				await _incidentService.CloseIncidentAsync(incidentId, updatedStatus);

				return RedirectToAction("IncidentDetails", new { id = incidentId });
			}
			catch (KeyNotFoundException ex)
			{
				TempData["Error"] = ex.Message;
				Console.WriteLine(ex);
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Could not close incident: {ex.Message}";
				Console.WriteLine(ex);
				return RedirectToAction("Index");
			}
		}

		[SessionAuthorize(UserType.Service_employee)]
		[HttpPost]
		/*
		public async Task<IActionResult> TransferIncident(string incidentId)
		{
			try
			{
				await _incidentService.TransferIncidentAsync(incidentId, newLocationBranchName);
				return RedirectToAction("IncidentDetails", new { id = incidentId });
			}
			catch (KeyNotFoundException ex)
			{
				TempData["Error"] = ex.Message;
				Console.WriteLine(ex);
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Could not transfer incident: {ex.Message}";
				Console.WriteLine(ex);
				return RedirectToAction("Index");
			}
		}*/


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
