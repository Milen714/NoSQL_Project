using ChapeauPOS.Commons;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
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
        public IncidentController(IUserService userService, ILocationService locationSrvice, IIncidentService incidentService, IIncidentService incidents)
        {
            _locationService = locationSrvice;
            _userService = userService;
            _incidentService = incidentService;
        }
        public async Task<IActionResult> Index(string searchString, int pageNumber
            , string currentFilter, string statusFilter, string typeFilter, string branch, bool showImmediateAttention = false)
        {
            List<Incident> incidents;
            int openIncidents = await _incidentService.GetTheNumberOfAllOpenIncidents();
            int numNonClosedIncidents = await _incidentService.GetTheNumberOfAllIncidents();
            bool hasStatus = !string.IsNullOrEmpty(statusFilter) && statusFilter != "All";
            bool hasType = !string.IsNullOrEmpty(typeFilter);
            string branchValue = !string.IsNullOrEmpty(branch) ? branch : "";
            List<Incident> immediateAttentionNeeded = await _incidentService.GetAllOpenOverdueIncidents(branch);

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
                    incidents = await _incidentService.GetIncidentsByStatusAndType(parsedStatus, parsedType, branchValue);
                }
                // Filter by status only
                else if (hasStatus &&
                    Enum.TryParse<IncidentStatus>(statusFilter, true, out parsedStatus))
                {
                    incidents = await _incidentService.GetAllIncidentsPerStatus(parsedStatus, branchValue);
                }
                // Filter by type only
                else if (hasType &&
                    Enum.TryParse<IncidentType>(typeFilter, true, out parsedType))
                {
                    incidents = await _incidentService.GetAllIncidentsByType(parsedType, branchValue);
                }
                // Show only incidents needing immediate attention
                else if (showImmediateAttention)
                {
                    incidents = immediateAttentionNeeded;
                }
                // No filters — show all
                else
                {
                    incidents = await _incidentService.GetAllWitoutclosed(branchValue);
                }
                ViewData["TypeFilter"] = typeFilter;
                ViewData["CurrentStatus"] = statusFilter;
                ViewData["NumberOfOpenIncidents"] = openIncidents;
                ViewData["NumberOfNonClosedIncidents"] = numNonClosedIncidents;
                ViewBag.Branches = _locationService.GetAllLocations().Result;
                ViewBag.ImmediateAttentionNeeded = immediateAttentionNeeded;


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
                Priority = Priority.medium,
                Deadline = 14,
                IncidentType = IncidentType.software,
            };

            var locations = await _locationService.GetAllLocations();
            ViewBag.Locations = locations.OrderBy(l => l.Branch).ToList();

            return View(viewModel);
        }

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

        [SessionAuthorize(UserType.Service_employee)]
        [HttpGet]
        public async Task<IActionResult> TransferIncident(string incidentId)
        {
            ViewBag.IncidentId = incidentId;

            var usersForTransfer = await _incidentService.GetUsersForTransferAsync();
            return View("TransferIncident", usersForTransfer);
        }

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

    }
}
