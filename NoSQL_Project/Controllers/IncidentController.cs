using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Models;
using System.Security.Claims;
using NoSQL_Project.ViewModels;
using NoSQL_Project.Repositories.Incidents;
using NoSQL_Project.Services.Interfaces;
using NoSQL_Project.Services.Incidents;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.Services;
using MongoDB.Bson;

namespace NoSQL_Project.Controllers
{
    public class IncidentController : Controller
    {
		private readonly IIncidentService _incidentService;
		private readonly IUserService _userService;

		public IncidentController(IIncidentService incidentService, IUserService userService)
		{
			_incidentService = incidentService;
			_userService = userService;
		}

		[HttpGet]
        public async Task<IActionResult> CreateIncident()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //if (string.IsNullOrEmpty(userId))
            //    return Unauthorized();

            var reporterUser = await _userService.GetReporterSnapshotAsync(userId);
			//if (reporterUser is null)
			// return Forbid(); // o maneja el caso con un mensaje
			// 
			

			var viewModel = new NewIncidentViewModel
            {
				Reporter = new ReporterSnapshot
				{
					UserId = ObjectId.Parse("68daf4894feb5ff46478b788"),
					FirstName = "William",
					LastName = "Kok"
				},
				//Reporter = reporterUser,

				// Defaults
				Priority = Priority.medium,
                Deadline = 14,
                IncidentType = IncidentType.software,           
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateIncident(NewIncidentViewModel model)
        {
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			_incidentService.CreateNewIncidentAsync(model);

			return RedirectToAction("Index", "Home"); 
		}
		
	}
}
