using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigPlus.Models
{
    public class ConfigurationOptions
    {
        public string? Environment { get; set; }

        public bool ValidateDataAnnotations { get; set; } = true;

        public bool ThrowOnError { get; set; } = false;

        public bool UseCache { get; set; } = true;

        public int CacheDurationSeconds { get; set; } = 300;

        public bool EnableHotReload { get; set; } = false;
    }
}
