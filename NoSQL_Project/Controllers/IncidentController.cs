using Microsoft.AspNetCore.Mvc;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Models;
using NoSQL_Project.Services.Interfaces;

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

                var incidents = _incidentService.GetAll().Where(i => i.Status == IncidentStatus.closed_without_resolve);

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
    }
}
