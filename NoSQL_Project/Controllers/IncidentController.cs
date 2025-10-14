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

        public IncidentController(IUserService userService, ILocationService locationService, IIncidentService incidentService)
        {
            _locationService = locationService;
            _userService = userService;
            _incidentService = incidentService;
        }

        // Enhanced Index supports all main features plus your reporter filter and search logic
        public async Task<IActionResult> Index(string searchString, int pageNumber = 1, string currentFilter = "", string statusFilter = "", string typeFilter = "", string branch = "", string sortOrder = "", string searchLogic = "or")
        {
            var user = HttpContext.Session.GetObject<User>("LoggedInUser");
            if (user == null)
                return RedirectToAction("Login", "Home");

            // Let main's filters drive the query, but add your reporterId logic when regular employee
            string reporterId = null;
            if (user.UserType == UserType.Reg_employee)
                reporterId = user.Id;

            int openIncidents = await _incidentService.GetTheNumberOfAllOpenIncidents();
            int numNonClosedIncidents = await _incidentService.GetTheNumberOfAllIncidents();
            bool hasStatus = !string.IsNullOrEmpty(statusFilter) && statusFilter != "All";
            bool hasType = !string.IsNullOrEmpty(typeFilter);
            string branchValue = !string.IsNullOrEmpty(branch) ? branch : "";

            // Use search logic param from your branch
            var logic = searchLogic.ToLower() == "and" ? SearchLogic.And : SearchLogic.Or;
            var sortType = sortOrder switch
            {
                "priority" => IncidentSort.PriorityAsc,
                "priority_desc" => IncidentSort.PriorityDesc,
                _ => IncidentSort.MostRecent
            };

            List<Incident> incidents;

            try
            {
                if (searchString != null && searchString != "")
                {
                    pageNumber = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                // Main's filtering over status/type/branch, plus reporterId 
                if (hasStatus && hasType &&
                    Enum.TryParse<IncidentStatus>(statusFilter, true, out var parsedStatus) &&
                    Enum.TryParse<IncidentType>(typeFilter, true, out var parsedType))
                {
                    incidents = await _incidentService.GetIncidentsByStatusAndType(parsedStatus, parsedType, branchValue);
                }
                else if (hasStatus &&
                    Enum.TryParse<IncidentStatus>(statusFilter, true, out parsedStatus))
                {
                    incidents = await _incidentService.GetAllIncidentsPerStatus(parsedStatus, branchValue);
                }
                else if (hasType &&
                    Enum.TryParse<IncidentType>(typeFilter, true, out parsedType))
                {
                    incidents = await _incidentService.GetAllIncidentsByType(parsedType, branchValue);
                }
                else if (!string.IsNullOrEmpty(searchString) || reporterId != null)
                {
                    // Your service logic for search and reporter filtering
                    incidents = await _incidentService.SearchIncidentsAsync(
                        searchString,
                        logic,
                        sortType,
                        reporterId
                    );
                }
                else
                {
                    incidents = await _incidentService.GetAllWitoutclosed(branchValue);
                }

                ViewData["TypeFilter"] = typeFilter;
                ViewData["CurrentStatus"] = statusFilter;
                ViewData["NumberOfOpenIncidents"] = openIncidents;
                ViewData["NumberOfNonClosedIncidents"] = numNonClosedIncidents;
                ViewBag.Branches = (await _locationService.GetAllLocations()).OrderBy(l => l.Branch).ToList();

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

        // GET: Create Incident, merged from both branches (includes location sorting from main)
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
                Priority = Priority.medium,
                Deadline = 14,
                IncidentType = IncidentType.software,
            };

            ViewBag.Locations = (await _locationService.GetAllLocations()).OrderBy(l => l.Branch).ToList();
            return View(viewModel);
        }

        // POST: Incident creation logic from main
        [HttpPost]
        public async Task<IActionResult> CreateIncident(NewIncidentViewModel newIncident)
        {
            try
            {
                Incident createdIncident = await _incidentService.CreateNewIncidentAsync(newIncident);
                return RedirectToAction("IncidentDetails", new { createdIncident.Id, isEditing = false });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Could not create incident: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // Details — merged, unchanged
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

        // Edit Incident: unchanged
        [SessionAuthorize(UserType.Service_employee)]
        public IActionResult EditIncident(string id)
        {
            return RedirectToAction("IncidentDetails", new { id, isEditing = true });
        }

        // Update Incident: combines ModelState checks from both branches
        [HttpPost]
        public async Task<IActionResult> UpdateIncident(Incident updatedIncident)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.IsEditing = true;
                    return View("IncidentDetails", updatedIncident);
                }

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

        // Close Incident (from main)
        [SessionAuthorize(UserType.Service_employee)]
        [HttpPost]
        public async Task<IActionResult> CloseIncident(string incidentId, string updatedStatus)
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

        // Transfer Incident GET
        [SessionAuthorize(UserType.Service_employee)]
        [HttpGet]
        public async Task<IActionResult> TransferIncident(string incidentId)
        {
            ViewBag.IncidentId = incidentId;
            var usersForTransfer = await _incidentService.GetUsersForTransferAsync();
            return View("TransferIncident", usersForTransfer);
        }

        // Transfer Incident POST
        [HttpPost]
        public async Task<IActionResult> TransferIncident(string incidentId, string userForTransferId)
        {
            try
            {
                await _incidentService.TransferIncidentAsync(incidentId, userForTransferId);
                return RedirectToAction("IncidentDetails", new { id = incidentId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        // Change Priority — from your branch
        [HttpPost]
        public IActionResult ChangePriority(string incidentId, Priority newPriority)
        {
            return RedirectToAction("IncidentDetails", new { id = incidentId });
        }

        // Change Status — from your branch
        [HttpPost]
        public IActionResult ChangeStatus(string incidentId, IncidentStatus status)
        {
            return RedirectToAction("IncidentDetails", new { id = incidentId });
        }
    }
}
