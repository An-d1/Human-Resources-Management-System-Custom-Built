using HRManagementSystem.Models;
using HRManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRManagementSystem.Controllers
{
    [Authorize(Roles = "MegaAdmin")]
    public class MegaAdminController : Controller
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly UserManager<User> _userManager;

        private readonly EmailServiceSender _emailSender;

        // everything is functional need to add authorization and stylidg
        public MegaAdminController(ICompanyRepository companyRepository, UserManager<User> userManager, EmailServiceSender emailServiceSender)
        {
            _companyRepository = companyRepository;
            _userManager = userManager;
            _emailSender = emailServiceSender;
        }

        public async Task<IActionResult> Index()
        {
            var companiesNotApprovedYet = await _companyRepository.GetAllCompaniesWhereApprovalStatusNullAsync();
            return View(companiesNotApprovedYet);
        }
        
        // methods to accept or reject companies in the wait-list
        [HttpPost]
        public async Task<IActionResult> ApproveCompany(Guid companyId)
        {
            var company = await _companyRepository.GetCompanyByIdAsync(companyId);
            if (company != null)
            {
                company.IsActive = true;
                company.ApprovalStatus = true;
                await _companyRepository.UpdateCompanyAsync(company);
            }

            var user = new User
            {
                FirstName = company.Name,
                LastName = "Admin",
                Email = company.CompanyEmail,
                UserName = company.CompanyEmail,
                PhoneNumber = company.CompanyPhoneNumber,
                CompanyId = company.Id
            };

            var temporaryPassword = $"{company.Name}A!@.3";

            var existing = await _userManager.FindByEmailAsync(user.Email);
            if (existing != null)
                return BadRequest("An account for this email already exists.");

            var createResult = await _userManager.CreateAsync(user, temporaryPassword);
            if (!createResult.Succeeded)
                return BadRequest(string.Join("; ", createResult.Errors.Select(e => e.Description)));

            var roleResult = await _userManager.AddToRoleAsync(user, "Admin"); // or "MegaAdmin"
            if (!roleResult.Succeeded)
                return BadRequest("Could not assign role to user.");

                // Generating token for email confirmation
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // Url builder to be sent to the users email address, along with the userId and the token so the user doesn't have to interact
                var confirmationLink = Url.Action("ConfirmEmail", "Email", new { userId = user.Id, token = token }, Request.Scheme);

                var companyName = await _companyRepository.GetCompanyNameByCompanyIdAsync(company.Id);


            #region the html email to be sent to the user

            string subject = "Welcome to SmartStaff — Your Company Has Been Approved";
            string htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        :root {{
                            --color-primary: #007bff;
                            --color-primary-hover: #0056b3;
                            --color-text: #343a40;
                            --color-bg-light: #f8f9fa;
                            --color-border: #e9ecef;
                        }}

                        body {{
                            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                            background-color: var(--color-bg-light);
                            color: var(--color-text);
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 40px auto;
                            padding: 30px;
                            background-color: #ffffff;
                            border-radius: 12px;
                            box-shadow: 0 6px 15px rgba(0, 0, 0, 0.08);
                            text-align: center;
                        }}
                        .header {{
                            border-bottom: 2px solid var(--color-primary);
                            padding-bottom: 20px;
                            margin-bottom: 20px;
                        }}
                        .header h1 {{
                            color: var(--color-primary);
                            font-size: 28px;
                            margin: 0;
                        }}
                        .content {{
                            padding: 10px 0;
                            line-height: 1.7;
                        }}
                        .credentials-box {{
                            background-color: var(--color-bg-light);
                            border: 1px solid var(--color-border);
                            border-radius: 8px;
                            padding: 15px;
                            margin: 20px 0;
                            text-align: left;
                            font-family: monospace;
                            font-size: 15px;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 14px 35px;
                            margin: 30px 0;
                            font-size: 17px;
                            font-weight: 600;
                            color: #ffffff;
                            background-color: var(--color-primary);
                            text-decoration: none;
                            border-radius: 8px;
                            transition: background-color 0.3s ease;
                        }}
                        .button:hover {{
                            background-color: var(--color-primary-hover);
                        }}
                        .footer {{
                            padding-top: 25px;
                            border-top: 1px solid var(--color-border);
                            font-size: 12px;
                            color: #6c757d;
                            line-height: 1.5;
                        }}
                        .disclaimer {{
                            font-style: italic;
                            margin-top: 20px;
                            color: #adb5bd;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Welcome to SmartStaff</h1>
                        </div>
                        <div class='content'>
                            <p>Dear {companyName},</p>
                            <p>
                                One of our administrators has <strong>reviewed and approved your company</strong> on SmartStaff.
                                Along with this email, you will find the <strong>temporary admin credentials</strong> that can be used 
                                to log in and begin setting up your organization.
                            </p>

                            <div class='credentials-box'>
                                <p><strong>Admin Username:</strong> {company.CompanyEmail}</p>
                                <p><strong>Temporary Password:</strong> {temporaryPassword}</p>
                            </div>

                            <p>
                                Before accessing your account, please confirm your email by clicking the button below:
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

                            <p><strong>Important:</strong> Please change your password immediately after logging in to ensure your account’s security.</p>

                            <p>
                                Once logged in, the admin user can create and manage departments, employees, and roles 
                                as needed to get your company started on SmartStaff.
                            </p>

                            <p class='disclaimer'>
                                If you did not request or expect this registration, please ignore this email.
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


            await _emailSender.SendEmailAsync(user.Email, subject, htmlBody);

            return RedirectToAction("ConfirmationLinkWasSent", "Email");

        }

        [HttpPost]
        public async Task<IActionResult> RejectCompany(Guid companyId)
        {
            var company = await _companyRepository.GetCompanyByIdAsync(companyId);
            if (company != null)
            {
                company.ApprovalStatus = false;
                await _companyRepository.UpdateCompanyAsync(company);

                // Send rejection email explaining the reason
                var companyName = await _companyRepository.GetCompanyNameByCompanyIdAsync(company.Id);

                #region the html email to be sent to the user

                string subject = "Your SmartStaff Application Has Been Reviewed";
                string htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        :root {{
                            --color-primary: #007bff;
                            --color-primary-hover: #0056b3;
                            --color-text: #343a40;
                            --color-bg-light: #f8f9fa;
                            --color-border: #e9ecef;
                        }}

                        body {{
                            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                            background-color: var(--color-bg-light);
                            color: var(--color-text);
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 40px auto;
                            padding: 30px;
                            background-color: #ffffff;
                            border-radius: 12px;
                            box-shadow: 0 6px 15px rgba(0, 0, 0, 0.08);
                            text-align: center;
                        }}
                        .header {{
                            border-bottom: 2px solid var(--color-primary);
                            padding-bottom: 20px;
                            margin-bottom: 20px;
                        }}
                        .header h1 {{
                            color: var(--color-primary);
                            font-size: 28px;
                            margin: 0;
                        }}
                        .content {{
                            padding: 10px 0;
                            line-height: 1.7;
                        }}
                        .credentials-box {{
                            background-color: var(--color-bg-light);
                            border: 1px solid var(--color-border);
                            border-radius: 8px;
                            padding: 15px;
                            margin: 20px 0;
                            text-align: left;
                            font-family: monospace;
                            font-size: 15px;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 14px 35px;
                            margin: 30px 0;
                            font-size: 17px;
                            font-weight: 600;
                            color: #ffffff;
                            background-color: var(--color-primary);
                            text-decoration: none;
                            border-radius: 8px;
                            transition: background-color 0.3s ease;
                        }}
                        .button:hover {{
                            background-color: var(--color-primary-hover);
                        }}
                        .footer {{
                            padding-top: 25px;
                            border-top: 1px solid var(--color-border);
                            font-size: 12px;
                            color: #6c757d;
                            line-height: 1.5;
                        }}
                        .disclaimer {{
                            font-style: italic;
                            margin-top: 20px;
                            color: #adb5bd;
                        }}
                    </style>
                </head>
                    <body>
                      <div class=""container"">
                        <h2>Application Review Outcome</h2>
                        <p>
                          Hello <strong>{companyName}</strong>,
                        </p>
                        <p>
                          One of our administrators has <strong>reviewed and therefore rejected your company application</strong> on <strong>SmartStaff</strong>.
                        </p>
                        <p>
                          Unfortunately, this means your organization will not be granted access to the SmartStaff platform at this time.
                          If you believe this decision was made in error or you wish to reapply, please contact our support team or submit a new application after addressing the feedback you received (if any).
                        </p>
                        <p>
                          We appreciate your interest in joining SmartStaff and wish you success in your future endeavors.
                        </p>
                        <div class=""footer"">
                          — The SmartStaff Team  
                        </div>
                      </div>
                    </body>
                </html>";

                #endregion
                await _emailSender.SendEmailAsync(company.CompanyEmail, subject, htmlBody);
            }
            return RedirectToAction("Index");
        }
    }
}
