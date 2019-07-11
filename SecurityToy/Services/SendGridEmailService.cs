using Microsoft.Extensions.Configuration;
using SecurityToy.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityToy.Services
{
    public class SendGridEmailService : IEmailService
    {
        private IConfiguration _configuration;  
        public SendGridEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmail(EmailTemplate emailTemplate)
        {
            var apiKey = _configuration["Keys:Sendgrid"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(emailTemplate.FromEmail, "Security Toy");
            var to = new EmailAddress(emailTemplate.ToEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, emailTemplate.Subject, emailTemplate.PlainText, emailTemplate.HtmlText);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
