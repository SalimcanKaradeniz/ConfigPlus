using ConfigPlus.Models;
using ConfigPlus.Tests.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace ConfigPlus.Tests
{
    public class ConfigManagerTests : IDisposable
    {
        private readonly IConfiguration _configuration;

        public ConfigManagerTests()
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

                ["InvalidSection:RequiredField"] = "", // Empty required field
                ["InvalidSection:RangeField"] = "50"    // Out of range (1-10)
            };

            _configuration = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

            ConfigManager.Initialize(_configuration);
        }

        public void Dispose()
        {

        }

        [Fact]
        public void GetValidConfigurationShouldReturnSuccess()
        {
            var result = ConfigManager.Get<DatabaseConfig>("Database");

            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.ConnectionString.Should().Be("Server=localhost;Database=TestDb");
            result.Value.TimeoutSeconds.Should().Be(60);
            result.Value.EnableRetry.Should().BeTrue();
            result.SectionPath.Should().Be("Database");
            result.Environment.Should().BeNull();
            result.ValidationErrors.Should().BeEmpty();
        }

        [Fact]
        public void GetNonExistentSectioShouldReturnFailure()
        {
            var result = ConfigManager.Get<DatabaseConfig>("NonExistent");

            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Value.Should().BeNull();
            result.ValidationErrors.Should().HaveCount(1);
            result.ValidationErrors.First().ErrorMessage.Should().Contain("not found");
            result.SectionPath.Should().Be("NonExistent");
        }

        [Fact]
        public void GetWithOptionsShouldApplyOptions()
        {
            var options = new ConfigurationOptions
            {
                ValidateDataAnnotations = false
            };

            var result = ConfigManager.Get<DatabaseConfig>("Database", options);

            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Value.Should().NotBeNull();
        }

        [Fact]
        public void GetForEnvironmentExistingEnvironmentShouldReturnEnvironmentSpecificConfig()
        {
            var result = ConfigManager.GetForEnvironment<DatabaseConfig>("Database", "Production");

            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.ConnectionString.Should().Be("Server=prod-server;Database=ProdDb");
            result.Value.TimeoutSeconds.Should().Be(120);
            result.Value.EnableRetry.Should().BeFalse();
            result.Environment.Should().Be("Production");
            result.SectionPath.Should().Be("Database");
        }

        [Fact]
        public void GetForEnvironmentNonExistentEnvironmentShouldFallbackToBase()
        {
            var result = ConfigManager.GetForEnvironment<DatabaseConfig>("Database", "Staging");

            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.ConnectionString.Should().Be("Server=localhost;Database=TestDb");
            result.Environment.Should().BeNull();
        }

        [Fact]
        public void GetForEnvironmentNonExistentSectionAndEnvironmentShouldReturnFailure()
        {
            var result = ConfigManager.GetForEnvironment<DatabaseConfig>("NonExistent", "Production");

            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Value.Should().BeNull();
            result.ValidationErrors.Should().HaveCount(1);
        }

        [Fact]
        public void GetValidatedValidConfigurationShouldReturnSuccessWithoutErrors()
        {
            var result = ConfigManager.GetValidated<EmailConfig>("Email");

            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.ValidationErrors.Should().BeEmpty();
            result.Value!.SmtpHost.Should().Be("smtp.gmail.com");
            result.Value.Port.Should().Be(587);
            result.Value.FromAddress.Should().Be("test@example.com");
        }

        [Fact]
        public void GetValidatedInvalidConfigurationShouldReturnValidationErrors()
        {
            var result = ConfigManager.GetValidated<InvalidConfig>("InvalidSection");

            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Value.Should().BeNull();
            result.ValidationErrors.Should().HaveCountGreaterThan(0);

            var errorMessages = result.ValidationErrors.Select(e => e.ErrorMessage).ToList();
            errorMessages.Should().Contain(msg => msg!.Contains("required") || msg.Contains("Required"));
            errorMessages.Should().Contain(msg => msg!.Contains("range") || msg.Contains("Range"));
        }

        [Fact]
        public void GetValidatedNonExistentSectionShouldReturnFailure()
        {
            var result = ConfigManager.GetValidated<EmailConfig>("NonExistent");

            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Value.Should().BeNull();
            result.ValidationErrors.Should().HaveCount(1);
            result.ValidationErrors.First().ErrorMessage.Should().Contain("not found");
        }

        [Fact]
        public void ValidateAllConfigurationsAllValidShouldReturnEmptyErrors()
        {
            var configSections = new Dictionary<string, Type>
            {
                { "Database", typeof(DatabaseConfig) },
                { "Email", typeof(EmailConfig) }
            };

            var errors = ConfigManager.ValidateAllConfigurations(configSections);

            errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateAllConfigurationsSomeInvalidShouldReturnErrors()
        {
            var configSections = new Dictionary<string, Type>
            {
                { "Database", typeof(DatabaseConfig) },
                { "InvalidSection", typeof(InvalidConfig) },
                { "NonExistent", typeof(EmailConfig) }
            };

            var errors = ConfigManager.ValidateAllConfigurations(configSections);

            errors.Should().HaveCount(2);
            errors.Should().Contain(e => e.SectionPath == "InvalidSection");
            errors.Should().Contain(e => e.SectionPath == "NonExistent");
        }

        [Fact]
        public void ValidateAllConfigurationsEmptyDictionaryShouldReturnEmptyErrors()
        {
            var configSections = new Dictionary<string, Type>();

            var errors = ConfigManager.ValidateAllConfigurations(configSections);

            errors.Should().BeEmpty();
        }
        
        [Fact]
        public void InitializeNullConfigurationShouldThrowArgumentNullException()
        {
            var act = () => ConfigManager.Initialize(null!);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("configuration");
        }

        [Fact]
        public void InitializeValidConfigurationShouldNotThrow()
        {
            var config = new ConfigurationBuilder().Build();

            var act = () => ConfigManager.Initialize(config);
            act.Should().NotThrow();
        }

    }
}