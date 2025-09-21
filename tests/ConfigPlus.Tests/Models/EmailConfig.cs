using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigPlus.Tests.Models
{
    public class EmailConfig
    {
        [Required]
        public string SmtpHost { get; set; } = string.Empty;

        [Range(1, 65535)]
        public int Port { get; set; } = 587;

        [Required]
        [EmailAddress]
        public string FromAddress { get; set; } = string.Empty;
    }
}
