using SecurityToy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityToy.Services
{
    public interface IEmailService
    {
        Task SendEmail(EmailTemplate emailTemplate);
    }
}
