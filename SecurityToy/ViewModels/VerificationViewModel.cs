using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityToy.ViewModels
{
    public class PhoneVerificationViewModel
    {
        public string  Phone { get; set; }
    }

    public class EmailVerificationViewModel
    {
        public string Email { get; set; }
    }

    public class OtpViewModel
    {
        public string Otp { get; set; }
    }
}
