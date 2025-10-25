namespace HRManagementSystem.ViewModels
{
    public class AdminViewModel
    {
        public string FirstName { get; set; } // Used only to pass the name of the user to the dashboard
        public string Email { get; set; }
        public int EmployeeCount { get; set; } // Used to pass the no of total employees to the model
        public int NewHires { get; set; } // Queries the db for all new users added within 31 days that belong to the same companyId as the admin
        public int PendingPTO { get; set; } // Queries the db for pending time off requests
    }
}
