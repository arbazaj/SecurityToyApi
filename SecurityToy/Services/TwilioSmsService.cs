using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.Extensions.Configuration;

namespace SecurityToy.Services
{
    public class TwilioSmsService : ISmsService
    {
        private IConfiguration _configuration;
        public TwilioSmsService(IConfiguration configuration)
        {

        }
        public Task<MessageResource> SendSmsAsync(string number, string message)
        {
            //Plug in your SMS service here to send a text message.
            // Your Account SID from twilio.com / console

            var accountSid =  _configuration["Keys:TwilioSid"];
            //Your Auth Token from twilio.com / console

            var authToken = _configuration["Keys:TwilioAuthToken"];

            TwilioClient.Init(accountSid, authToken);

            return MessageResource.CreateAsync(
              to: new PhoneNumber(number),
              from: new PhoneNumber(_configuration["Keys:TwilioPhoneNumber"]),
              body: message);
        }
    }
}
