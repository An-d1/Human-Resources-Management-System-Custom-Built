using HRManagementSystem.Models;
using HRManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HRManagementSystem.Controllers
{
    public class LogInController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public LogInController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginUser(LogInViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email); // user can sign in by the email address

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

                    if (result.Succeeded && User.IsInRole("Employee"))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else if (result.Succeeded && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (result.Succeeded && User.IsInRole("MegaAdmin"))
                    {
                        return RedirectToAction("Index", "MegaAdmin");
                    }
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View("Index", model);
        }

        // Logout 
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }


    }
}
