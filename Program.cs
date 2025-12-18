using HRManagementSystem.Data;
using HRManagementSystem.Models;
using HRManagementSystem.Repositories;
using HRManagementSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
namespace HRManagementSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region configurations for the custom FluentEmail sender
            // Reading smtp configuration for mailsending from appsettings.json
            var smtp = builder.Configuration.GetSection("SmtpSettings");

            builder.Services
                .AddFluentEmail(smtp["User"]) // default sender
                .AddRazorRenderer()
                .AddSmtpSender(() => new SmtpClient(smtp["Host"], int.Parse(smtp["Port"]))
                {
                    Credentials = new NetworkCredential(smtp["User"], smtp["Password"]),
                    EnableSsl = true
                });
            builder.Services.AddScoped<EmailServiceSender>();

            // Service to create secure random temporary passwords
            builder.Services.AddScoped<TemporaryPasswordGenerator>();
            #endregion

            // Add DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Model Services
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            builder.Services.AddScoped<IHolidayCalendarRepository, HolidayRepository>();
            builder.Services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
            builder.Services.AddScoped<IPayRateRepository, PayRateRepository>();    
            builder.Services.AddScoped<IEmployeeDepartmentRepository, EmployeeDepartmentRepository>();
            builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();


            // Nager to get national holidays
            builder.Services.AddHttpClient<NationalHolidayService>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add Identity
            builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Login/Index";          // Redirect to this when not logged in
                options.AccessDeniedPath = "/Login/AccessDenied";
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();  
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=LandingPage}/{action=Index}/{id?}"); // So the user is always prompted with the log in page

            app.MapControllers();
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            // Applies migrations
            context.Database.Migrate();

            // here goes the seed for the db
            // Avoids reseeding if company already exists
            if (!context.Companies.Any())
            {
                // - Company 

                var hrCompany = new Company
                {
                    Id = Guid.Parse("552809FC-7190-42B7-B666-B168851385C5"),
                    Name = "HRManagementSystem",
                    CompanyEmail = "hrmanagementsystem@gmail.com",
                    CompanyPhoneNumber = "06811111111"
                };

                var company = new Company
                {
                    Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                    Name = "***Company***",
                    CompanyEmail = "prg@gmail.com",
                    CompanyPhoneNumber = "0671111111"
                };
                context.Companies.AddRange(hrCompany, company);

                // - Departments 
                var hrDept = new Department { Id = Guid.Parse("b1111111-1111-1111-1111-111111111111"), Name = "HR", CompanyId = company.Id };
                var itDept = new Department { Id = Guid.Parse("b2222222-2222-2222-2222-222222222222"), Name = "IT", CompanyId = company.Id };
                var financeDept = new Department { Id = Guid.Parse("b3333333-3333-3333-3333-333333333333"), Name = "Finance", CompanyId = company.Id };

                context.Departments.AddRange(hrDept, itDept, financeDept);

                // ROLES via RoleManager 
                var roles = new[] {"MegaAdmin", "Admin", "Manager", "Finance", "Employee" };
                foreach (var roleName in roles)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                        await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }


                // - Users 
                var hasher = new PasswordHasher<User>();

                var megaAdmin = new User
                {
                    Id = Guid.Parse("c5555555-5555-5555-5555-555555555555"),
                    UserName = "megaadmin@company.com",
                    NormalizedUserName = "MEGAADMIN@COMPANY.COM",
                    Email = "megaadmin@company.com",
                    NormalizedEmail = "MEGAADMIN@COMPANY.COM",
                    EmailConfirmed = true,
                    FirstName = "SystemMega",
                    LastName = "Admin",
                    CompanyId = Guid.Parse("552809FC-7190-42B7-B666-B168851385C5"),
                    PasswordHash = hasher.HashPassword(null!, "MegaAdmin123!"),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow,
                };

                var admin = new User
                {
                    Id = Guid.Parse("c1111111-1111-1111-1111-111111111111"),
                    UserName = "admin@company.com",
                    NormalizedUserName = "ADMIN@COMPANY.COM",
                    Email = "admin@company.com",
                    NormalizedEmail = "ADMIN@COMPANY.COM",
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Admin",
                    CompanyId = company.Id,
                    PasswordHash = hasher.HashPassword(null!, "Admin123!"),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow,
                };

                var manager = new User
                {
                    Id = Guid.Parse("c2222222-2222-2222-2222-222222222222"),
                    UserName = "manager@company.com",
                    NormalizedUserName = "MANAGER@COMPANY.COM",
                    Email = "manager@company.com",
                    NormalizedEmail = "MANAGER@COMPANY.COM",
                    EmailConfirmed = true,
                    FirstName = "John",
                    LastName = "Manager",
                    CompanyId = company.Id,
                    PasswordHash = hasher.HashPassword(null!, "Manager123!"),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow,
                };

                var employee = new User
                {
                    Id = Guid.Parse("c3333333-3333-3333-3333-333333333333"),
                    UserName = "employee@company.com",
                    NormalizedUserName = "EMPLOYEE@COMPANY.COM",
                    Email = "employee@company.com",
                    NormalizedEmail = "EMPLOYEE@COMPANY.COM",
                    EmailConfirmed = true,
                    FirstName = "Jane",
                    LastName = "Employee",
                    CompanyId = company.Id,
                    PasswordHash = hasher.HashPassword(null!, "Employee123!"),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow,
                };

                var finance = new User
                {
                    Id = Guid.Parse("c4444444-4444-4444-4444-444444444444"),
                    UserName = "finance@company.com",
                    NormalizedUserName = "FINANCE@COMPANY.COM",
                    Email = "finance@company.com",
                    NormalizedEmail = "FINANCE@COMPANY.COM",
                    EmailConfirmed = true,
                    FirstName = "Fiona",
                    LastName = "Finance",
                    CompanyId = company.Id,
                    PasswordHash = hasher.HashPassword(null!, "Finance123!"),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow,
                };

                context.Users.AddRange(megaAdmin, admin, manager, employee, finance);

                // employee table that contains employee specific data
                var managerFromUsers = new Employee
                {
                    UserId = manager.Id,
                    HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    IsEmployed = true,
                };
                var financeFromUsers = new Employee
                {
                    UserId = finance.Id,
                    HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    IsEmployed = true,
                };
                var employeeFromUsers = new Employee
                {
                    UserId = employee.Id,
                    HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    IsEmployed = true,
                };

                context.Employees.AddRange(managerFromUsers, financeFromUsers, employeeFromUsers);

                // - EmployeeDepartment relationships 
                context.EmployeeDepartments.AddRange(
                    new EmployeeDepartment { UserId = admin.Id, DepartmentId = itDept.Id },
                    new EmployeeDepartment { UserId = manager.Id, DepartmentId = hrDept.Id },
                    new EmployeeDepartment { UserId = employee.Id, DepartmentId = itDept.Id },
                    new EmployeeDepartment { UserId = finance.Id, DepartmentId = financeDept.Id }
                );

                // Save users & deps first so UserManager can attach them
                await context.SaveChangesAsync();

                // ASSIGN USERS TO ROLES via UserManager 
                await userManager.AddToRoleAsync(megaAdmin, "MegaAdmin");
                await userManager.AddToRoleAsync(admin, "Admin");
                await userManager.AddToRoleAsync(manager, "Manager");
                await userManager.AddToRoleAsync(finance, "Finance");
                await userManager.AddToRoleAsync(employee, "Employee");

                Console.WriteLine("Seeded: Company, Departments, Roles, Users, Role assignments, and EmployeeDepartment links.");
            }
            else
            {
                Console.WriteLine("Seed skipped — data already exists.");
            }

            app.Run();

        }

    }
}

