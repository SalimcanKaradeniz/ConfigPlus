using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ConfigPlus.Models;
using ConfigPlus.Exceptions;

namespace ConfigPlus.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfigPlus(this IServiceCollection services, IConfiguration configuration, ConfigurationOptions? options = null)
        {
            ConfigManager.Initialize(configuration, options);

            services.AddSingleton(configuration);
            services.AddSingleton(options ?? new ConfigurationOptions());

            return services;
        }

        public static IServiceCollection ConfigureFromConfigPlus<T>(this IServiceCollection services, string sectionPath, ConfigurationOptions? options = null) where T : class, new()
        {
            services.AddSingleton<T>(serviceProvider =>
            {
                var result = ConfigManager.GetValidated<T>(sectionPath, options);

                if (!result.IsValid)
                {
                    var errors = string.Join(", ", result.ValidationErrors.Select(e => e.ErrorMessage));
                    throw new ConfigurationException(sectionPath, options?.Environment, $"Configuration validation failed: {errors}");
                }

                return result.Value;
            });

            return services;
        }

        public static IServiceCollection ConfigureFromConfigPlusForEnvironment<T>(this IServiceCollection services, string sectionPath, string environment, ConfigurationOptions? options = null) where T : class, new()
        {
            var environmentOptions = options ?? new ConfigurationOptions();
            environmentOptions.Environment = environment;

            return services.ConfigureFromConfigPlus<T>(sectionPath, environmentOptions);
        }

        public static IServiceCollection ValidateConfigurations(this IServiceCollection services, Dictionary<string, Type> configurationSections)
        {
            var errors = ConfigManager.ValidateAllConfigurations(configurationSections);

            if (errors.Any())
            {
                var errorMessage = string.Join(Environment.NewLine, errors.Select(e => $"Section '{e.SectionPath}': {e.Message}"));

                throw new AggregateException($"Configuration validation failed:{Environment.NewLine}{errorMessage}", errors);
            }

            return services;
        }
    }
}
