using HRManagementSystem.Data;
using HRManagementSystem.Models;
using HRManagementSystem.Services;
using HRManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace HRManagementSystem.Controllers
{
    [Authorize(Roles = "Admin, MegaAdmin")]
    public class RegisterController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        private readonly ApplicationDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly EmailServiceSender _emailSender;
        public RegisterController(UserManager<User> userManager, SignInManager<User> signInManager,
            ApplicationDbContext context, IUserRepository userRepository, ICompanyRepository companyRepository,
            EmailServiceSender emailSender)

        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _emailSender = emailSender;
        }

        // New employee registration
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewUserAccountFromAdmin(RegisterEmployeeViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User); // Get the current user

            Guid adminId = currentUser.Id; // Get the current Admin Id

            var companyId = await _userRepository.GetCompanyIdByUserIdAsync(adminId); // Get his Id so it can be assigned as the employees Id also

            if (ModelState.IsValid)
            {
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    CompanyId = companyId.Value,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password); // Create

                if (result.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, model.Role);

                    if (!roleResult.Succeeded)
                    {
                        ModelState.AddModelError("", "Could not assign role to user.");
                    }

                    // Generating token for email confirmation
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    // Url builder to be sent to the users email address, along with the userId and the token so the user doesn't have to interact
                    var confirmationLink = Url.Action("ConfirmEmail", "Email", new { userId = user.Id, token = token }, Request.Scheme);

                    var companyName = await _companyRepository.GetCompanyNameByCompanyIdAsync(companyId.Value);

                    #region the html email to be sent to the user

                    string subject = "Welcome to SmartStaff — Confirm Your Employee Account";
                    string htmlBody = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <style>
                                /* Define Professional Color Palette */
                                :root {{
                                    --color-primary: #007bff; /* Deep Blue for Action */
                                    --color-primary-hover: #0056b3; /* Darker Blue for Hover */
                                    --color-text: #343a40; /* Dark Gray for main text */
                                    --color-bg-light: #f8f9fa; /* Very Light Gray background */
                                    --color-border: #e9ecef; /* Light Border */
                                }}
            
                                body {{
                                    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; /* Added Inter for theme consistency */
                                    background-color: var(--color-bg-light); /* Very light background */
                                    color: var(--color-text); /* Dark text */
                                    margin: 0;
                                    padding: 0;
                                }}
                                .container {{
                                    max-width: 600px;
                                    margin: 40px auto;
                                    padding: 30px; /* Increased padding */
                                    background-color: #ffffff;
                                    border-radius: 12px;
                                    box-shadow: 0 6px 15px rgba(0, 0, 0, 0.08); /* Softer, slightly stronger shadow */
                                    text-align: center;
                                }}
                                .header {{
                                    /* Change border color to a professional accent blue */
                                    border-bottom: 2px solid var(--color-primary); 
                                    padding-bottom: 20px;
                                    margin-bottom: 20px;
                                }}
                                .header h1 {{
                                    color: var(--color-primary); /* Use primary blue for the title */
                                    font-size: 28px;
                                    margin: 0;
                                }}
                                .content {{
                                    padding: 10px 0;
                                    line-height: 1.7; /* Better readability */
                                }}
                                .button {{
                                    display: inline-block;
                                    padding: 14px 35px; /* Larger button for tap targets */
                                    margin: 30px 0; /* More vertical space */
                                    font-size: 17px;
                                    font-weight: 600; /* Semi-bold text */
                                    color: #ffffff;
                                    background-color: var(--color-primary); /* Primary blue */
                                    text-decoration: none;
                                    border-radius: 8px;
                                    transition: background-color 0.3s ease;
                                    /* Important for email rendering: inline styles for button */
                                    mso-padding-alt: 14px 35px; 
                                }}
                                .button:hover {{
                                    background-color: var(--color-primary-hover);
                                }}
                                .footer {{
                                    padding-top: 25px;
                                    border-top: 1px solid var(--color-border);
                                    font-size: 12px;
                                    color: #6c757d; /* Muted dark gray */
                                    line-height: 1.5;
                                }}
                                .disclaimer {{
                                    font-style: italic;
                                    margin-top: 20px;
                                    color: #adb5bd; /* Muted light gray for less important text */
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    <h1>Welcome to SmartStaff</h1>
                                </div>
                                <div class='content'>
                                    <p>Dear {user.FirstName},</p>
                                    <p>
                                        An administrator from <strong>{companyName}</strong> has created an employee account for you on <strong>SmartStaff</strong>
                                        using this email address. Please confirm your email below to **activate your account** and start using the portal.
                                    </p>
                
                                    <table role='presentation' cellspacing='0' cellpadding='0' border='0' align='center'>
                                        <tr>
                                            <td style='border-radius: 8px; background-color: #007bff; mso-padding-alt: 14px 35px;' align='center'>
                                                 <a href='{confirmationLink}' class='button' 
                                                    style='color: #ffffff; display: block; text-decoration: none; border-radius: 8px;'>
                                                    Confirm My Account
                                                 </a>
                                            </td>
                                        </tr>
                                    </table>
                
                                    <p class='disclaimer'>
                                        If you did not expect this invitation or believe this was sent by mistake, please ignore this email.
                                    </p>
                                </div>
                                <div class='footer'>
                                    <p>&copy; 2025 SmartStaff. All rights reserved.</p>
                                    <p style='margin-top: 5px;'>Please do not reply to this automated email.</p>
                                </div>
                            </div>
                        </body>
                        </html>";
                    #endregion

                    // The email service set up sending the confirmation link for the user
                    await _emailSender.SendEmailAsync(
                        user.Email,
                        subject,
                        htmlBody
                    );

                    // Redirect the user to the success page.
                    return RedirectToAction("ConfirmationLinkWasSent", "Email");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View("Index", model);

        }

        //

        #region Here goes the code for the company Inquiry/Creation that will be handled by the general Admin of the system
        [AllowAnonymous]
        public IActionResult InquirySuccessfulView()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult CompanyRegister()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SendCompanyApplication(RegisterCompanyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var Company = new Company()
                {
                    Name = model.CompanyName,
                    IsActive = false,
                    ApprovalStatus = null, // means pending
                    CompanyEmail = model.Email,
                    CompanyPhoneNumber = model.PhoneNumber,
                };

                if (model.TaxId != null)
                {
                    Company.TaxId = model.TaxId;
                }

                #region the html email to be sent to the user

                string subject = "Your Company Application Has Been Received – SmartStaff";
                string htmlBody = $@"
                <!DOCTYPE html>
                    <html>
                    <head>
                      <meta charset=""UTF-8"">
                      <style>
                        body {{
                          font-family: 'Inter', Arial, sans-serif;
                          color: #333;
                          background-color: #f8f9fa;
                          padding: 30px;
                        }}
                        .container {{
                          max-width: 600px;
                          margin: auto;
                          background: #fff;
                          border-radius: 10px;
                          padding: 30px;
                          box-shadow: 0 2px 10px rgba(0,0,0,0.05);
                        }}
                        h2 {{
                          color: #222;
                          font-weight: 600;
                        }}
                        p {{
                          line-height: 1.6;
                          font-size: 15px;
                        }}
                        .footer {{
                          font-size: 13px;
                          color: #777;
                          margin-top: 25px;
                          text-align: center;
                        }}
                      </style>
                    </head>
                    <body>
                      <div class=""container"">
                        <h2>Application Successfully Submitted</h2>
                        <p>
                          Hello <strong>{Company.Name}</strong>,
                        </p>
                        <p>
                          We’ve successfully received your company application on <strong>SmartStaff</strong>.
                        </p>
                        <p>
                          Our administrative team will carefully review your submission. You’ll receive an update via email once a decision has been made.  
                          Please note that this process may take up to a few business days depending on the verification workload.
                        </p>
                        <p>
                          Thank you for your interest in joining SmartStaff. We appreciate your patience and look forward to reviewing your application.
                        </p>
                        <div class=""footer"">
                          — The SmartStaff Team  
                          <br><a href=""https://smartstaff.app"" style=""color:#007bff;text-decoration:none;"">smartstaff.app</a>
                        </div>
                      </div>
                    </body>
                    </html>
                    ";

                #endregion
                await _emailSender.SendEmailAsync(Company.CompanyEmail, subject, htmlBody);
                
                await _companyRepository.AddCompanyAsync(Company);

                return RedirectToAction("InquirySuccessfulView", "Register");
            }

            return View(model);

        }
            #endregion

        }
}
