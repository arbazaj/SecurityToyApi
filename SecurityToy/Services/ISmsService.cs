using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;

namespace SecurityToy.Services
{
    public interface ISmsService
    {
        Task<MessageResource> SendSmsAsync(string number, string message);
    }
}
