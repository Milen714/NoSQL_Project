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
