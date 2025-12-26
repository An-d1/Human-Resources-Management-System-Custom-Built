using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HRManagementSystem.Models;

namespace HRManagementSystem.ViewComponents
{
    public class UserHeaderViewComponent : ViewComponent
    {
        private readonly UserManager<User> _userManager;

        public UserHeaderViewComponent(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // parameters here to receive data from the page
        public async Task<IViewComponentResult> InvokeAsync(string title, string subtitle)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var viewModel = new HeaderComponentViewModel
            {
                User = user,
                Title = title,
                Subtitle = subtitle
            };

            return View(viewModel);
        }
    }

    //  helper class for the component
    public class HeaderComponentViewModel
    {
        public User User { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
    }
}