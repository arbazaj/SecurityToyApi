using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityToy.Models
{
    public class VerificationToken
    {
        [Key]
        public string Token { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public bool IsActive { get; set; }

        public  DateTime CreatedOn { get; set; }

        public DateTime ExpiresOn { get; set; }

    }
}
