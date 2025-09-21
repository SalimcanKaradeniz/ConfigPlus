using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ConfigPlus.Extensions;
using ConfigPlus.Tests.Models;
using ConfigPlus.Exceptions;

namespace ConfigPlus.Tests
{
    public class ExtensionMethodsTests : IDisposable
    {
        private readonly IConfiguration _configuration;

        public ExtensionMethodsTests()
        {
            var configData = new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = "Server=localhost;Database=TestDb",
                ["Database:TimeoutSeconds"] = "60",
                ["Database:EnableRetry"] = "true",

                ["Database_Production:ConnectionString"] = "Server=prod-server;Database=ProdDb",
                ["Database_Production:TimeoutSeconds"] = "120",
                ["Database_Production:EnableRetry"] = "false",

                ["Email:SmtpHost"] = "smtp.gmail.com",
                ["Email:Port"] = "587",
                ["Email:FromAddress"] = "test@example.com",

                ["InvalidSection:RequiredField"] = "",
                ["InvalidSection:RangeField"] = "50"
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }

        public void Dispose()
        {
        }

        [Fact]
        public void AddConfigPlusValidConfigurationShouldRegisterServices()
        {
            var services = new ServiceCollection();

            services.AddConfigPlus(_configuration);
            var serviceProvider = services.BuildServiceProvider();

            var registeredConfig = serviceProvider.GetService<IConfiguration>();
            registeredConfig.Should().NotBeNull();
            registeredConfig.Should().Be(_configuration);
        }

        [Fact]
        public void AddConfigPlusWithOptionsShouldRegisterServicesWithOptions()
        {
            var services = new ServiceCollection();
            var options = new ConfigPlus.Models.ConfigurationOptions
            {
                ValidateDataAnnotations = true,
                ThrowOnError = false
            };

            services.AddConfigPlus(_configuration, options);
            var serviceProvider = services.BuildServiceProvider();

            var registeredConfig = serviceProvider.GetService<IConfiguration>();
            var registeredOptions = serviceProvider.GetService<ConfigPlus.Models.ConfigurationOptions>();

            registeredConfig.Should().NotBeNull();
            registeredOptions.Should().NotBeNull();
            registeredOptions!.ValidateDataAnnotations.Should().BeTrue();
            registeredOptions.ThrowOnError.Should().BeFalse();
        }

        [Fact]
        public void ConfigureFromConfigPlusValidConfigurationShouldRegisterConfigurationClass()
        {
            var services = new ServiceCollection();
            services.AddConfigPlus(_configuration);

            services.ConfigureFromConfigPlus<DatabaseConfig>("Database");
            var serviceProvider = services.BuildServiceProvider();

            var dbConfig = serviceProvider.GetService<DatabaseConfig>();
            dbConfig.Should().NotBeNull();
            dbConfig!.ConnectionString.Should().Be("Server=localhost;Database=TestDb");
            dbConfig.TimeoutSeconds.Should().Be(60);
            dbConfig.EnableRetry.Should().BeTrue();
        }

        [Fact]
        public void ConfigureFromConfigPlusInvalidConfigurationShouldThrowConfigurationException()
        {
            var services = new ServiceCollection();
            services.AddConfigPlus(_configuration);
            services.ConfigureFromConfigPlus<InvalidConfig>("InvalidConfig");

            var act = () => services.BuildServiceProvider().GetService<InvalidConfig>();
            act.Should().Throw<ConfigurationException>()
                .WithMessage("*validation failed*");
        }

        [Fact]
        public void ConfigureFromConfigPlusNonExistentSectionShouldThrowConfigurationException()
        {
            var services = new ServiceCollection();
            services.AddConfigPlus(_configuration);
            services.ConfigureFromConfigPlus<DatabaseConfig>("NonExistentSection");

            var act = () => services.BuildServiceProvider().GetRequiredService<DatabaseConfig>();

            act.Should().Throw<ConfigurationException>()
                .WithMessage("*validation failed*");
        }

        [Fact]
        public void ConfigureFromConfigPlusMultipleConfigurationsShouldRegisterAll()
        {
            var services = new ServiceCollection();
            services.AddConfigPlus(_configuration);

            services.ConfigureFromConfigPlus<DatabaseConfig>("Database");
            services.ConfigureFromConfigPlus<EmailConfig>("Email");
            var serviceProvider = services.BuildServiceProvider();

            var dbConfig = serviceProvider.GetService<DatabaseConfig>();
            var emailConfig = serviceProvider.GetService<EmailConfig>();

            dbConfig.Should().NotBeNull();
            emailConfig.Should().NotBeNull();
            dbConfig!.ConnectionString.Should().Be("Server=localhost;Database=TestDb");
            emailConfig!.SmtpHost.Should().Be("smtp.gmail.com");
        }

        [Fact]
        public void ConfigureFromConfigPlusForEnvironmentExistingEnvironmentShouldUseEnvironmentSpecificConfig()
        {
            var services = new ServiceCollection();
            services.AddConfigPlus(_configuration);

            services.ConfigureFromConfigPlusForEnvironment<DatabaseConfig>("Database", "Production");
            var serviceProvider = services.BuildServiceProvider();

            var dbConfig = serviceProvider.GetService<DatabaseConfig>();
            dbConfig.Should().NotBeNull();
            dbConfig!.ConnectionString.Should().Be("Server=prod-server;Database=ProdDb");
            dbConfig.TimeoutSeconds.Should().Be(120);
            dbConfig.EnableRetry.Should().BeFalse();
        }

        [Fact]
        public void ConfigureFromConfigPlusForEnvironmentNonExistentEnvironmentShouldFallbackToBase()
        {
            var services = new ServiceCollection();
            services.AddConfigPlus(_configuration);

            services.ConfigureFromConfigPlusForEnvironment<DatabaseConfig>("Database", "Staging");
            var serviceProvider = services.BuildServiceProvider();

            var dbConfig = serviceProvider.GetService<DatabaseConfig>();
            dbConfig.Should().NotBeNull();
            dbConfig!.ConnectionString.Should().Be("Server=localhost;Database=TestDb");
            dbConfig.TimeoutSeconds.Should().Be(60);
            dbConfig.EnableRetry.Should().BeTrue();
        }


        [Fact]
        public void ValidateConfigurationsAllValidShouldNotThrow()
        {
            var services = new ServiceCollection();
            services.AddConfigPlus(_configuration);

            var configSections = new Dictionary<string, Type>
            {
                { "Database", typeof(DatabaseConfig) },
                { "Email", typeof(EmailConfig) }
            };

            var act = () => services.ValidateConfigurations(configSections);
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateConfigurationsSomeInvalidShouldThrowAggregateException()
        {
            var services = new ServiceCollection();
            services.AddConfigPlus(_configuration);

            var configSections = new Dictionary<string, Type>
            {
                { "Database", typeof(DatabaseConfig) },
                { "InvalidConfig", typeof(InvalidConfig) },
                { "NonExistent", typeof(EmailConfig) }
            };

            var act = () => services.ValidateConfigurations(configSections);
            act.Should().Throw<AggregateException>()
                .WithMessage("*Configuration validation failed*");
        }

        [Fact]
        public void ValidateConfigurationsEmptyDictionaryShouldNotThrow()
        {
            var services = new ServiceCollection();
            services.AddConfigPlus(_configuration);

            var configSections = new Dictionary<string, Type>();

            var act = () => services.ValidateConfigurations(configSections);
            act.Should().NotThrow();
        }

        [Fact]
        public void FullIntegrationCompleteWorkflowShouldWorkEndToEnd()
        {
            var services = new ServiceCollection();

            services.AddConfigPlus(_configuration);
            services.ConfigureFromConfigPlus<DatabaseConfig>("Database");
            services.ConfigureFromConfigPlusForEnvironment<EmailConfig>("Email", "Development");

            var configSections = new Dictionary<string, Type>
            {
                { "Database", typeof(DatabaseConfig) },
                { "Email", typeof(EmailConfig) }
            };

            services.ValidateConfigurations(configSections);
            var serviceProvider = services.BuildServiceProvider();

            var dbConfig = serviceProvider.GetService<DatabaseConfig>();
            var emailConfig = serviceProvider.GetService<EmailConfig>();

            dbConfig.Should().NotBeNull();
            emailConfig.Should().NotBeNull();
            dbConfig!.ConnectionString.Should().NotBeNullOrEmpty();
            emailConfig!.SmtpHost.Should().NotBeNullOrEmpty();
        }
    }
}