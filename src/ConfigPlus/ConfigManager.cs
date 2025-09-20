using ConfigPlus.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigPlus
{
    public static class ConfigManager
    {
        private static IConfiguration? _configuration;
        private static ConfigurationOptions _globalOptions = new();

        public static void Initialize(IConfiguration configuration, ConfigurationOptions options = null) 
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _globalOptions = options ?? new ConfigurationOptions();  
        } 
    }
}