using ConfigPlus.Exceptions;
using ConfigPlus.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public static ConfigurationResult<T> Get<T>(string sectionPath, ConfigurationOptions options = null) where T : class, new()
        {
            if (_configuration == null)
                throw new InvalidOperationException("ConfigManager is not initialized. Call Initialize() method first.");

            var effectiveOptions = options ?? _globalOptions;

            var effectivePath = BuildEffectivePath(sectionPath, effectiveOptions.Environment);

            var section = _configuration.GetSection(effectivePath);
            if (!section.Exists() && !string.IsNullOrEmpty(effectiveOptions.Environment))
            {
                section = _configuration.GetSection(sectionPath);
                effectiveOptions.Environment = null;
            }

            if (!section.Exists())
                return ConfigurationResult<T>.Failure(
                    new[]
                    { new ValidationResult($"Configuration section '{sectionPath}' not found")},
                    sectionPath,
                    effectiveOptions.Environment);

            try
            {
                var configValue = new T();
                section.Bind(configValue);

                if (effectiveOptions.ValidateDataAnnotations)
                {
                    var validationResults = ValidateConfiguration(configValue);
                    if (validationResults.Any())
                        return ConfigurationResult<T>.Failure(validationResults, sectionPath, effectiveOptions.Environment);
                }

                return ConfigurationResult<T>.Success(configValue, sectionPath, null);
            }
            catch (Exception ex)
            {
                return ConfigurationResult<T>.Failure(
                    new[]
                    { new ValidationResult($"Configuration binding error: {ex.Message}") },
                    sectionPath,
                    null);
            }
        }

        public static ConfigurationResult<T> GetForEnvironment<T>(string sectionPath, string environment, ConfigurationOptions options = null) where T : class, new()
        {
            var envOptions = options ?? new ConfigurationOptions();
            envOptions.Environment = environment;

            return Get<T>(sectionPath, envOptions);
        }

        public static ConfigurationResult<T> GetValidated<T>(string sectionPath, ConfigurationOptions? options = null) where T : class, new()
        {
            var validationOptions = options ?? new ConfigurationOptions();
            validationOptions.ThrowOnError = true;

            return Get<T>(sectionPath, validationOptions);
        }

        public static List<ConfigurationException> ValidateAllConfigurations(Dictionary<string, Type> configurationSections)
        {
            var errors = new List<ConfigurationException>();

            foreach (var (sectionPath, configType) in configurationSections)
            {
                try
                {
                    var method = typeof(ConfigManager).GetMethod(nameof(GetValidated))!.MakeGenericMethod(configType);
                    var result = method.Invoke(null, new object[] { sectionPath, null });

                    var isValidProperty = result!.GetType().GetProperty(nameof(ConfigurationResult<object>.IsValid))!;
                    var isValid = (bool)isValidProperty.GetValue(result)!;

                    if (!isValid) 
                    {
                        var errorsProperty = result.GetType().GetProperty(nameof(ConfigurationResult<object>.ValidationErrors))!;
                        var validationErrors = (IReadOnlyList<ValidationResult>)errorsProperty.GetValue(result)!;

                        var errorMessages = string.Join(", ", validationErrors.Select(e => e.ErrorMessage));
                        errors.Add(new ConfigurationException(sectionPath, $"Validation failed: {errorMessages}"));
                    }

                }
                catch (Exception ex)
                {
                    errors.Add(new ConfigurationException(sectionPath, $"Configuration error: {ex.Message}", ex));
                }
            }

            return errors;
        }

        #region Private Methods
        private static string BuildEffectivePath(string sectionPath, string? environment)
        {
            if (string.IsNullOrEmpty(environment))
                return sectionPath;

            return $"{sectionPath}_{environment}";
        }

        private static List<ValidationResult> ValidateConfiguration<T>(T value) where T : class
        {
            var context = new ValidationContext(value);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(value, context, results, validateAllProperties: true);

            return results;
        }
        #endregion
    }
}