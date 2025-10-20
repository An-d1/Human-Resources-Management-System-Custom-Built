using HRManagementSystem.Models;
using HRManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HRManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        public AdminController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            string userId = _userManager.GetUserId(User);
            var customUser = await _userManager.FindByIdAsync(userId);
            
            var model = new LogInViewModel
            {
                FirstName = customUser.FirstName
            };

            return View(model);
        }
    }
}
