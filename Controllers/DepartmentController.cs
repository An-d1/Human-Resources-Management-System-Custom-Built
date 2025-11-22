using Microsoft.AspNetCore.Mvc;

namespace HRManagementSystem.Controllers
{
    public class DepartmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
