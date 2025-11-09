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
        private readonly IIncidentSearchService _searchService;
        private readonly IIncidentSortService _sortService;
        private readonly IArchiveIncidentService _archiveIncidentService;

        public IncidentController(
            IUserService userService,
            ILocationService locationSrvice,
            IIncidentService incidentService,
            IIncidentSearchService searchService,
            IIncidentSortService sortService,
            IArchiveIncidentService archiveIncidentService)
        {
            _locationService = locationSrvice;
            _userService = userService;
            _incidentService = incidentService;
            _searchService = searchService;
            _sortService = sortService;
            _archiveIncidentService = archiveIncidentService;
        }

        public async Task<IActionResult> Index(
     string searchString,
     string searchOperator,
     bool sortByPriority = false,
     bool sortPriorityAscending = false,
     int pageNumber = 1,
     string currentFilter = "",
     string statusFilter = "",
     string typeFilter = "",
     string branch = "",
     bool showImmediateAttention = false)
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

                // Parse status and type filters
                IncidentStatus? parsedStatus = null;
                IncidentType? parsedType = null;

                if (hasStatus && Enum.TryParse<IncidentStatus>(statusFilter, true, out var tempStatus))
                {
                    parsedStatus = tempStatus;
                }

                if (hasType && Enum.TryParse<IncidentType>(typeFilter, true, out var tempType))
                {
                    parsedType = tempType;
                }

                //SEARCH FUNCTIONALITY

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    var searchOp = searchOperator?.ToLower() == "or" ? SearchOperator.Or : SearchOperator.And;


                    incidents = await _searchService.SearchIncidentsAsync(
                        searchString,
                        searchOp,
                        branchValue,
                        parsedStatus,
                        parsedType
                    );

                    ViewData["CurrentFilter"] = searchString;
                    ViewData["SearchOperator"] = searchOperator?.ToLower() ?? "and";
                }
                //SORT FUNCTIONALITY 
                else if (sortByPriority)
                {

                    incidents = await _sortService.GetIncidentsSortedByPriorityAsync(branchValue, sortPriorityAscending);

                    ViewData["SortByPriority"] = true;
                    ViewData["SortPriorityAscending"] = sortPriorityAscending;
                }

                else if (hasStatus && hasType)
                {
                    incidents = await _incidentService.GetIncidentsByStatusAndType(parsedStatus.Value, parsedType.Value, branchValue);
                }
                else if (hasStatus)
                {
                    incidents = await _incidentService.GetAllIncidentsPerStatus(parsedStatus.Value, branchValue);
                }
                else if (hasType)
                {
                    incidents = await _incidentService.GetAllIncidentsByType(parsedType.Value, branchValue);
                }
                else if (showImmediateAttention)
                {
                    incidents = immediateAttentionNeeded;
                }
                else
                {
                    incidents = await _incidentService.GetAllWitoutclosed(branchValue);
                }
                var awaitingArchival = await _incidentService.GetAwaitingToBeArchivedIncidents();
                ViewData["AwaitingArchival"] = awaitingArchival.Count();
                ViewData["TypeFilter"] = typeFilter;
                ViewData["CurrentStatus"] = statusFilter;
                ViewData["NumberOfOpenIncidents"] = openIncidents;
                ViewData["NumberOfNonClosedIncidents"] = numNonClosedIncidents;
                ViewBag.Branches = _locationService.GetAllLocations().Result;
                ViewBag.ImmediateAttentionNeeded = immediateAttentionNeeded;
                ViewData["NumberImmediateAttentionNeeded"] = immediateAttentionNeeded.Count();

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
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Could not close incident: {ex.Message}";
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

        /// <summary>
        /// Shows a regular employee's own tickets only.
        /// Reuses existing search/sort/filter logic but filters by logged-in user.
        /// Author: Dylan Mohlen
        /// Date: 2025-01-08
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MyTickets(
            string searchString,
            string searchOperator,
            bool sortByPriority = false,
            bool sortPriorityAscending = false,
            int pageNumber = 1,
            string currentFilter = "",
            string statusFilter = "")
        {
            // Get logged-in user
            var loggedInUser = HttpContext.Session.GetObject<User>("LoggedInUser");

            if (loggedInUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<Incident> incidents;

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


                IncidentStatus? parsedStatus = null;
                if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
                {
                    if (Enum.TryParse<IncidentStatus>(statusFilter, true, out var tempStatus))
                    {
                        parsedStatus = tempStatus;
                    }
                }


                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    var searchOp = searchOperator?.ToLower() == "or" ? SearchOperator.Or : SearchOperator.And;


                    var searchResults = await _searchService.SearchIncidentsAsync(
                        searchString,
                        searchOp,
                        "",
                        parsedStatus,
                        null);


                    incidents = FilterByUser(searchResults, loggedInUser);

                    ViewData["CurrentFilter"] = searchString;
                    ViewData["SearchOperator"] = searchOperator?.ToLower() ?? "and";
                }

                else if (sortByPriority)
                {
                    var allIncidents = await _incidentService.GetAllWitoutclosed("");
                    var myIncidents = FilterByUser(allIncidents, loggedInUser);

                    // Apply sorting
                    incidents = sortPriorityAscending
                        ? myIncidents.OrderByDescending(i => (int)i.Priority).ThenByDescending(i => i.ReportedAt).ToList()
                        : myIncidents.OrderBy(i => (int)i.Priority).ThenByDescending(i => i.ReportedAt).ToList();

                    ViewData["SortByPriority"] = true;
                    ViewData["SortPriorityAscending"] = sortPriorityAscending;
                }

                else if (parsedStatus.HasValue)
                {
                    var allIncidents = await _incidentService.GetAllIncidentsPerStatus(parsedStatus.Value, "");
                    incidents = FilterByUser(allIncidents, loggedInUser);
                }
                else
                {

                    var allIncidents = await _incidentService.GetAllWitoutclosed("");
                    incidents = FilterByUser(allIncidents, loggedInUser);
                }


                var allMyIncidents = FilterByUser(_incidentService.GetAll(), loggedInUser);
                ViewData["TotalTickets"] = allMyIncidents.Count;
                ViewData["OpenTickets"] = allMyIncidents.Count(i => i.Status == IncidentStatus.open || i.Status == IncidentStatus.inProgress);
                ViewData["ResolvedTickets"] = allMyIncidents.Count(i => i.Status == IncidentStatus.resolved || i.Status == IncidentStatus.closed);
                ViewData["ClosedWithoutResolve"] = allMyIncidents.Count(i => i.Status == IncidentStatus.closed_without_resolve);
                ViewData["CurrentStatus"] = statusFilter;

                if (pageNumber < 1)
                {
                    pageNumber = 1;
                }

                int pageSize = 10;
                return View(PaginatedList<Incident>.CreateAsync(incidents, pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Could not retrieve your tickets: {ex.Message}";
                return View(new PaginatedList<Incident>(new List<Incident>(), 0, 1, 1));
            }
        }

        private List<Incident> FilterByUser(List<Incident> incidents, User user)
        {
            return incidents
                .Where(i => i.ReportedBy != null &&
                           i.ReportedBy.FirstName == user.FirstName &&
                           i.ReportedBy.LastName == user.LastName)
                .ToList();
        }
        public async Task<IActionResult> DisplayAwaitingArchival(bool archive)
        {
            int pageSize = 5;
            try
            {
                var awaitingArchivalIncidents = await _incidentService.GetAwaitingToBeArchivedIncidents();
                pageSize += awaitingArchivalIncidents.Count();

                return PartialView("_IncidentsToBeArchived", PaginatedList<Incident>.CreateAsync(awaitingArchivalIncidents, 1, pageSize));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Could not retrieve archived incidents: {ex.Message}";
                List<Incident> empty = new List<Incident>();
                return PartialView("_IncidentsToBeArchived", PaginatedList<Incident>.CreateAsync(empty, 1, pageSize));
            }
        }
        public async Task<IActionResult> ArchiveOldIncidents()
        {
            try
            {
                var awaitingArchivalIncidents = await _incidentService.GetAwaitingToBeArchivedIncidents();
                await _archiveIncidentService.ArchiveOldIncidentsAsync(awaitingArchivalIncidents);
                await _incidentService.DeleteArchivedIncidents(awaitingArchivalIncidents);
                TempData["Success"] = "Old incidents archived successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Could not archive incidents: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        public async Task<IActionResult> DisplayArchivedIncidents()
        {
            int pageSize = 5;
            try
            {
                var archivedIncidents = await _archiveIncidentService.GetArchivedIncidents();
                pageSize += archivedIncidents.Count();
                return PartialView("_IncidentsToBeArchived", PaginatedList<Incident>.CreateAsync(archivedIncidents, 1, pageSize));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Could not retrieve archived incidents: {ex.Message}";
                List<Incident> empty = new List<Incident>();
                return PartialView("_IncidentsToBeArchived", PaginatedList<Incident>.CreateAsync(empty, 1, pageSize));
            }
        }

    }
}
