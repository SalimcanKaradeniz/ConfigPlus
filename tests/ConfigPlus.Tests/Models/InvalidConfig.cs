using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigPlus.Tests.Models
{
    public class InvalidConfig
    {
        [Required]
        public string RequiredField { get; set; } = string.Empty;

        [Range(1, 10)]
        public int RangeField { get; set; }
    }
}
