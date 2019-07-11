using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityToy.Models
{
    public class User
    {
        [Key]
        public string  UserId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string Phone { get; set; }

        public bool IsActive { get; set; }

        public bool IsTwoStepVerificationEnabled { get; set; }

        public bool IsEmailVerified { get; set; }

        public bool IsPhoneVerified { get; set; }

        public string  Role { get; set; }

        public DateTime Created { get; set; }
    }
}
