using System;
using System.Collections.Generic;
using System.Text;

namespace EmailValidator.Models
{
    public class EmailResult
    {
        public string EmailAddress { get; set; }
        public bool IsValidRegex { get; set; }
        public bool IsValidSmtp { get; set; }

        public EmailResult(string email, bool regex, bool smtp)
        {
            EmailAddress = email;
            IsValidRegex = regex;
            IsValidSmtp = smtp;
        }
    }
}
