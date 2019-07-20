using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityToy.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Password { get; set; }

        public string Otp { get; set; }
    }

    public class TwoFactorOtpViewModel
    {
        public string Phone { get; set; }

        public string Email { get; set; }
    }
}
