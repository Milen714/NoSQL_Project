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
        private readonly IIncidentService _incidents;
        
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

                //incidents = _incidentService.GetAllIncidentsPerStatus(IncidentStatus.open, "").Result;

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
                return View(new PaginatedList<Incident>(new List<Incident>(), 0, 1, 1));
            }
        }

        public async Task<IActionResult> IncidentDetails(string id)
        {
            try
            {
                Incident incident = await _incidentService.GetIncidentByIdAsync(id);
                if (incident == null)
                {
                    TempData["Error"] = "Incident not found.";
                    return RedirectToAction("Index");
                }
                return View(incident);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Could not retrieve incident details: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public IActionResult Escalate(string incidentId, Priority escalateTo)
        {
            return RedirectToAction("IncidentDetails", new {id = incidentId});
        }
        [HttpPost]
        public IActionResult ChangeStatus(string incidentId, IncidentStatus status)
        {
            return RedirectToAction("IncidentDetails", new { id = incidentId });
        }
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

        [HttpPost]
        public async Task<IActionResult> CreateIncident(NewIncidentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _incidentService.CreateNewIncidentAsync(model);

            return RedirectToAction("Index", "Home");
        }
    }
}
