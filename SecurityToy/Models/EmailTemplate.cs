using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityToy.Models
{
    public class EmailTemplate
    {
        public string ToEmail { get; set; }

        public string FromEmail { get; set; }

        public string Subject { get; set; }

        public string PlainText { get; set; }

        public string HtmlText { get; set; }
    }
}
