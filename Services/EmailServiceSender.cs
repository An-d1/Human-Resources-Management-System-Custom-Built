using FluentEmail.Core;
namespace HRManagementSystem.Services
{
    public class EmailServiceSender
    {
        private readonly IFluentEmail _fluentEmail;

        public EmailServiceSender(IFluentEmail fluentEmail)
        {
            _fluentEmail = fluentEmail;
        }

        // Method that send an email with given body and given header
        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            var response = await _fluentEmail
                .To(to)
                .Subject(subject)
                .Body(body, isHtml: true) // Currently set as HTML, if to send email as plain text, set false
                .SendAsync();

            return response.Successful;
        }
    }
}
