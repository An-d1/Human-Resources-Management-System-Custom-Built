using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

namespace HRManagementSystem.Models;

[Index(nameof(CompanyEmail), IsUnique = true)] // so email and phone number have to be unique on the db
[Index(nameof(CompanyPhoneNumber), IsUnique = true)] // so email and phone number have to be unique on the db

public class Company
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? TaxId { get; set; } // NIPT in Albania
    public bool IsActive { get; set; } // 1 -> active ; 0 -> not active
    public bool? ApprovalStatus { get; set; } // NULL for pending, 0 for rejected, 1 for accepted 
    public string CompanyEmail { get; set; }
    public string CompanyPhoneNumber { get; set; }
    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<User> Employees { get; set; } = new List<User>(); 
}
