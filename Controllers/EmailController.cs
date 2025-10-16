using HRManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HRManagementSystem.Controllers
{
    public class EmailController : Controller
    {
        private readonly UserManager<User> _userManager;

        public EmailController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        public IActionResult ConfirmationLinkWasSent()
        {
            return View();
        }

        // This nmethod will handle the confirmation link.
        [HttpGet]
        public async Task<IActionResult> ConfirmEmailAsync(string userId, string token)
        {
            // Check if the userId and token are valid.
            if (userId == null || token == null)
            {
                // If not, show an error page.
                return View("Error");
            }

            // Find the user in the database using the userId.
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }

            //Confirm the user's email using the token.
            var result = await _userManager.ConfirmEmailAsync(user, token);

            //Handle the result of the confirmation.
            if (result.Succeeded)
            {
                // If successful, show a confirmation success page.
                return View("EmailConfirmationSuccess");
            }

            // If it failed for any reason (e.g., token expired), show an error page.
            return View("Error");
        }

        public IActionResult EmailConfirmationSuccess()
        {
            return View();
        }
    }
}
