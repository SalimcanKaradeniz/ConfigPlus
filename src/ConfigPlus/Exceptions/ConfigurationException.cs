using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigPlus.Exceptions
{
    public class ConfigurationException : Exception
    {
        public string SectionPath { get; }

       
        public string? Environment { get; }

        public ConfigurationException(string sectionPath, string message)
            : base(message)
        {
            SectionPath = sectionPath;
        }

        public ConfigurationException(string sectionPath, string message, Exception innerException)
            : base(message, innerException)
        {
            SectionPath = sectionPath;
        }

        public ConfigurationException(string sectionPath, string? environment, string message)
            : base(message)
        {
            SectionPath = sectionPath;
            Environment = environment;
        }

        public ConfigurationException(string sectionPath, string? environment, string message, Exception innerException)
            : base(message, innerException)
        {
            SectionPath = sectionPath;
            Environment = environment;
        }
    }
}
