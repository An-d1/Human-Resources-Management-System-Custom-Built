using HRManagementSystem.Data;
using HRManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRManagementSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add Identity
            builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
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

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Login}/{action=Index}/{id?}"); // So the user is always prompted with the log in page


            await SeedDataAsync(app); // to set the initial seed for the db
            app.Run();

        }

        public static async Task SeedDataAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var sp = scope.ServiceProvider;

            var context = sp.GetRequiredService<ApplicationDbContext>();
            var userManager = sp.GetRequiredService<UserManager<User>>();
            var roleManager = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            await context.Database.MigrateAsync();

            // Company
            var company = new Company { Id = Guid.NewGuid(), Name = "***Company*** Ltd" };
            context.Companies.Add(company);
            await context.SaveChangesAsync();

            // Departments
            var hr = new Department { Id = Guid.NewGuid(), Name = "Human Resources", CompanyId = company.Id };
            var it = new Department { Id = Guid.NewGuid(), Name = "IT", CompanyId = company.Id };
            var fin = new Department { Id = Guid.NewGuid(), Name = "Finance", CompanyId = company.Id };

            context.Departments.AddRange(hr, it, fin);
            await context.SaveChangesAsync();

            // Roles
            var adminRole = "Admin";
            var employeeRole = "Employee";
            if (!await roleManager.RoleExistsAsync(adminRole))
                await roleManager.CreateAsync(new IdentityRole<Guid>(adminRole));
            if (!await roleManager.RoleExistsAsync(employeeRole))
                await roleManager.CreateAsync(new IdentityRole<Guid>(employeeRole));

            // Admin
            var adminEmail = "admin@hrportal.com";
            var admin = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                CompanyId = company.Id,
                DepartmentId = hr.Id
            };
            await userManager.CreateAsync(admin, "Admin@1234");
            await userManager.AddToRoleAsync(admin, adminRole);

            // Employee
            var empEmail = "john.doe@hrportal.com";
            var employee = new User
            {
                UserName = empEmail,
                Email = empEmail,
                FirstName = "John",
                LastName = "Doe",
                EmailConfirmed = true,
                CompanyId = company.Id,
                DepartmentId = it.Id
            };
            await userManager.CreateAsync(employee, "Employee@123");
            await userManager.AddToRoleAsync(employee, employeeRole);

            // Many-to-many link 
            context.EmployeeDepartments.Add(new EmployeeDepartment { UserId = employee.Id, DepartmentId = it.Id });
            await context.SaveChangesAsync();
        }


    }
}

