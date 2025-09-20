using System.ComponentModel.DataAnnotations;

namespace ConfigPlus.Models
{
    public class ConfigurationResult<T> where T : class
    {
        public T? Value { get; init; }

        public bool IsValid => ValidationErrors.Count == 0;

        public IReadOnlyList<ValidationResult> ValidationErrors { get; init; } = Array.Empty<ValidationResult>();

        public string SectionPath { get; init; } = string.Empty;

        public string? Environment { get; init; }

        internal static ConfigurationResult<T> Success(T value, string sectionPath, string? environment = null)
        {
            return new ConfigurationResult<T>
            {
                Value = value,
                SectionPath = sectionPath,
                Environment = environment,
                ValidationErrors = Array.Empty<ValidationResult>()
            };
        }

        internal static ConfigurationResult<T> Failure(IEnumerable<ValidationResult> validationErrors, string sectionPath, string? environment = null)
        {
            return new ConfigurationResult<T>
            {
                Value = null,
                SectionPath = sectionPath,
                Environment = environment,
                ValidationErrors = validationErrors.ToList().AsReadOnly()
            };
        }
    }
}
