using HRManagementSystem.Models;
using HRManagementSystem.Services;
using HRManagementSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HRManagementSystem.Controllers
{
    public class ForgotResetPasswordController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly EmailServiceSender _emailServiceSender;

        public ForgotResetPasswordController(UserManager<User> userManager, EmailServiceSender emailServiceSender)
        {
            _userManager = userManager;
            _emailServiceSender = emailServiceSender;
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateAndSendPasswordResetToken(ForgotPasswordViewModel model)
        {
            // Find the user by the email address
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return View("Error");
            }

            // generate the token for the user if it's not null
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Generate an link to send to the users email
            var passwordResetLink = Url.Action("ResetPasswordSetNewOne", "ForgotResetPassword", new { userId = user.Id, token = token }, Request.Scheme);

            string subject = "SmartStaff — Reset Your Password";
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
                            font-size: 16px;
                            text-align: left;
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
                            mso-padding-alt: 14px 35px;
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
                            text-align: center;
                        }}

                        .disclaimer {{
                            font-style: italic;
                            margin-top: 20px;
                            color: #adb5bd;
                            font-size: 13px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Password Reset Request</h1>
                        </div>

                        <div class='content'>
                            <p>Dear {user.FirstName},</p>

                            <p>
                                A request was made to reset the password for your 
                                <strong>SmartStaff</strong> account.  
                                If this was you, please click the button below to create a new password.
                            </p>

                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' align='center'>
                                <tr>
                                    <td style='border-radius: 8px; background-color: #007bff; mso-padding-alt: 14px 35px;' align='center'>
                                        <a href='{passwordResetLink}' class='button'
                                            style='color: #ffffff; display: block; text-decoration: none; border-radius: 8px;'>
                                            Reset My Password
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <p class='disclaimer'>
                                If you did not request a password reset, you can safely ignore this email.  
                                No changes will be made to your account.
                            </p>
                        </div>

                        <div class='footer'>
                            <p>&copy; 2025 SmartStaff. All rights reserved.</p>
                            <p style='margin-top: 5px;'>Please do not reply to this automated email.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await _emailServiceSender.SendEmailAsync(
            user.Email,
            subject,
            htmlBody);

            // Redirect the user to the CheckYourEmail page after successful email sending.
            return RedirectToAction("PasswordResetEmailLinkWasSent", "Email");
        }

        [HttpGet]
        public IActionResult ResetPasswordSetNewOne(string userId, string token)
        {
            if (userId == null || token == null) return View("Error");

            return View(new ResetPasswordViewModel { UserId = userId, Token = token });  // Pass the needed userId and token to the viewModel
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordUser(ResetPasswordViewModel viewModel)
        {

            // Check if the userId and token are valid.
            if (viewModel.UserId == null || viewModel.Token == null)
            {
                // If not, show an error page.
                return View("Error");
            }

            // Find the user in the database using the userId.
            var user = await _userManager.FindByIdAsync(viewModel.UserId);
            if (user == null)
            {
                return View("Error");
            }

            //Reset the user's password using the token.
            var result = await _userManager.ResetPasswordAsync(user, viewModel.Token, viewModel.NewPassword);

            //Handle the result of the confirmation.
            if (result.Succeeded)
            {
                // If successful, show a confirmation success page.
                return RedirectToAction("PasswordResetSuccessPage");
            }

            // If it failed for any reason (e.g., token expired), show an error page.
            return View("Error");
        }

        public IActionResult PasswordResetSuccessPage()
        {
            return View();
        }

    }
}

