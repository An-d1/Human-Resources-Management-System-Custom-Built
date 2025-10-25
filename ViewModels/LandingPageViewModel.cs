namespace HRManagementSystem.ViewModels
{
    public class LandingPageViewModel
    {
        // Number of registered companies using the platform
        public string TotalCompanies { get; set; }

        // Number of employees managed across all companies
        public string TotalEmployees { get; set; }

        // Total amount of payroll processed (can be formatted in the view)
        public string TotalPayrollProcessed { get; set; }

        // Accuracy rate as a percentage (e.g. 99.9)
        public double AccuracyRate { get; set; }

        // Optional: dynamic list of feature cards for the “features” section
        public List<FeatureItem>? Features { get; set; }
    }

    public class FeatureItem
    {
        public string Icon { get; set; } = string.Empty;      // e.g. "👥" or a Bootstrap icon
        public string Title { get; set; } = string.Empty;     // e.g. "Automated Payroll"
        public string Description { get; set; } = string.Empty;
    }
}
