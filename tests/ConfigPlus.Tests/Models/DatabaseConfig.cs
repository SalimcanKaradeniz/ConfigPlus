using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigPlus.Tests.Models
{
    public class DatabaseConfig
    {
        [Required]
        public string ConnectionString { get; set; } = string.Empty;

        [Range(1, 3600)]
        public int TimeoutSeconds { get; set; } = 30;

        public bool EnableRetry { get; set; } = true;
    }
}
