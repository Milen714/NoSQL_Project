using Microsoft.AspNetCore.Mvc;

namespace NoSQL_Project.Controllers
{
    public class IncidentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
