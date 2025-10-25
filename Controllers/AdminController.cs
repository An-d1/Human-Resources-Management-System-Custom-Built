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
        private readonly IUserRepository _userRepository;
        public AdminController(UserManager<User> userManager, IUserRepository userRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
        }
        public async Task<IActionResult> Index()
        {
            string userId = _userManager.GetUserId(User);
            var customUser = await _userManager.FindByIdAsync(userId);
            
            var model = new AdminViewModel
            {
                FirstName = customUser.FirstName,
                EmployeeCount = await _userRepository.GetTheTotalNumberOfEmployeesUnderTheCompanyAsync(customUser.CompanyId),
                NewHires = await _userRepository.GetRecentHiresInTheLast30DaysAsync(customUser.CompanyId),
            };

            return View(model);
        }
    }
}
