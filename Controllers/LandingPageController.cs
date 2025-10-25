using HRManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HRManagementSystem.Controllers
{
    public class LandingPageController : Controller
    {
        public IActionResult Index()
        {
            var model = new LandingPageViewModel
            {
                TotalCompanies = "100K",
                TotalEmployees = "1.5M",
                TotalPayrollProcessed = "15M",
                AccuracyRate = 100,
                Features = new List<FeatureItem>
                {
                    new FeatureItem { Icon = "👥", Title = "Centralized Staff Management", Description = "Keep all employee records in one place." },
                    new FeatureItem { Icon = "⚡", Title = "Automated Payroll", Description = "Process salaries in minutes with zero errors." },
                    new FeatureItem { Icon = "📊", Title = "Analytics Dashboard", Description = "Visualize payroll costs in real time." }
                }
            };

            return View(model);
        }
    }
}
