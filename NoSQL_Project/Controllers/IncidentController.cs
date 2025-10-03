using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Bson;
using NoSQL_Project.Commons;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.Services;
using NoSQL_Project.Services.Interfaces;
using NoSQL_Project.ViewModels;
using System.Security.Claims;

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
			var user = HttpContext.Session.GetObject<User>("LoggedInUser");
			if (user == null)
			{
				return Unauthorized();
			}

			var reporterUser = new ReporterSnapshot
			{
				UserId = ObjectId.Parse(user.Id),
				FirstName = user.FirstName,
				LastName = user.LastName,
				EmailAddress = user.EmailAddress
			};

			var viewModel = new NewIncidentViewModel
			{
				Reporter = reporterUser,

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

			await _incidentService.CreateNewIncidentAsync(model);

			return RedirectToAction("Index", "Home"); 
		}
		
	}
}
