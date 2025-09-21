# ConfigPlus

**ConfigPlus** is a lightweight .NET library that provides type-safe, environment-aware configuration management with built-in validation support for ASP.NET Core applications.

## 🎯 Why ConfigPlus?

### The Problem
```csharp
// ❌ Traditional approach - Risky and error-prone
var connectionString = Configuration["Database:ConnectionString"]; // Can be null
var timeout = int.Parse(Configuration["Database:Timeout"]); // Can throw exception
// Production crash at 2 AM: "Email configuration missing!" 💥
```

### The Solution
```csharp
// ✅ ConfigPlus approach - Type-safe and predictable
var result = ConfigManager.GetValidated<DatabaseConfig>("Database");
if (result.IsValid)
{
    var dbConfig = result.Value; // Never null, IntelliSense available
    // All configuration errors caught at startup! 🚀
}
```

## 🌟 Key Features

- **🛡️ Type-Safe Access** - Strong typing with IntelliSense support
- **🌍 Environment-Aware** - Automatic environment-specific configuration handling
- **✅ Built-in Validation** - Data annotation support with detailed error reporting
- **🚀 Startup Validation** - Catch configuration errors before they reach production
- **🔌 DI Integration** - Seamless ASP.NET Core dependency injection support
- **📦 Zero Dependencies** - Only uses Microsoft.Extensions.* packages
- **⚡ Lightweight** - Minimal performance overhead
- **🔄 Auto Fallback** - Environment-specific to base configuration fallback

## 📦 Installation

```bash
dotnet add package ConfigPlus
```

Or via Package Manager Console:
```powershell
Install-Package ConfigPlus
```

## 🚀 Quick Start

### 1. Define Your Configuration Classes

```csharp
public class DatabaseConfig
{
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
    
    [Range(1, 3600)]
    public int TimeoutSeconds { get; set; } = 30;
    
    public bool EnableRetry { get; set; } = true;
}

public class EmailConfig
{
    [Required]
    [EmailAddress]
    public string SmtpHost { get; set; } = string.Empty;
    
    [Range(1, 65535)]
    public int Port { get; set; } = 587;
}
```

### 2. Configure Your appsettings.json

```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=MyApp",
    "TimeoutSeconds": 30,
    "EnableRetry": true
  },
  "Database_Production": {
    "ConnectionString": "Server=prod-server;Database=MyApp",
    "TimeoutSeconds": 60,
    "EnableRetry": true
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "Port": 587
  }
}
```

### 3. Setup in Program.cs (ASP.NET Core)

```csharp
using ConfigPlus.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add ConfigPlus
builder.Services.AddConfigPlus(builder.Configuration);

// Register configurations with validation
builder.Services.ConfigureFromConfigPlus<DatabaseConfig>("Database");
builder.Services.ConfigureFromConfigPlus<EmailConfig>("Email");

// Validate all configurations at startup
var configSections = new Dictionary<string, Type>
{
    { "Database", typeof(DatabaseConfig) },
    { "Email", typeof(EmailConfig) }
};

try
{
    builder.Services.ValidateConfigurations(configSections);
}
catch (AggregateException ex)
{
    Console.WriteLine("❌ Configuration validation failed:");
    foreach (var error in ex.InnerExceptions)
        Console.WriteLine($"  • {error.Message}");
    Environment.Exit(1);
}

var app = builder.Build();
```

### 4. Use in Controllers

```csharp
[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    private readonly DatabaseConfig _dbConfig;
    private readonly EmailConfig _emailConfig;
    
    public HomeController(DatabaseConfig dbConfig, EmailConfig emailConfig)
    {
        _dbConfig = dbConfig; // Automatically injected and validated!
        _emailConfig = emailConfig;
    }
    
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { 
            Database = _dbConfig.ConnectionString,
            Email = _emailConfig.SmtpHost 
        });
    }
}
```

## 📚 Usage Examples

### Basic Configuration Access
```csharp
// Initialize ConfigManager
ConfigManager.Initialize(configuration);

// Get configuration with validation
var result = ConfigManager.GetValidated<DatabaseConfig>("Database");
if (result.IsValid)
{
    var config = result.Value;
    // Use your config safely
}
```

### Environment-Specific Configuration
```csharp
// Get production-specific config
var prodResult = ConfigManager.GetForEnvironment<DatabaseConfig>("Database", "Production");

// Automatic fallback: if "Database_Production" doesn't exist, uses "Database"
var config = prodResult.Value;
```

### Manual Validation
```csharp
var result = ConfigManager.Get<DatabaseConfig>("Database");
if (!result.IsValid)
{
    foreach (var error in result.ValidationErrors)
    {
        Console.WriteLine($"Validation Error: {error.ErrorMessage}");
    }
}
```

## 🌍 Environment Support

ConfigPlus automatically handles environment-specific configurations:

- **Development**: `Database_Development` → fallback to `Database`
- **Production**: `Database_Production` → fallback to `Database`  
- **Staging**: `Database_Staging` → fallback to `Database`

## ✅ Validation Support

Built-in support for System.ComponentModel.DataAnnotations:

- `[Required]` - Ensures non-null/empty values
- `[Range(min, max)]` - Validates numeric ranges
- `[EmailAddress]` - Validates email format
- `[Url]` - Validates URL format
- `[MinLength]` / `[MaxLength]` - String length validation
- Custom validation attributes

## 🛠️ Advanced Configuration

### Custom Options
```csharp
var options = new ConfigurationOptions
{
    ValidateDataAnnotations = true,
    ThrowOnError = false,
    StrictMode = true
};

var result = ConfigManager.Get<DatabaseConfig>("Database", options);
```

### Environment-Specific Registration
```csharp
if (builder.Environment.IsProduction())
{
    builder.Services.ConfigureFromConfigPlusForEnvironment<DatabaseConfig>("Database", "Production");
}
else
{
    builder.Services.ConfigureFromConfigPlus<DatabaseConfig>("Database");
}
```

## 🎯 Benefits

### For Developers
- **🕐 Time Saving** - No more configuration boilerplate code
- **🐛 Bug Prevention** - Catch configuration errors at compile time
- **🧠 Peace of Mind** - Type-safe, predictable configuration access

### For Projects  
- **🚀 Reliability** - No more production configuration crashes
- **⚡ Performance** - Efficient configuration loading and caching
- **🔧 Maintainability** - Standardized configuration management approach

## 📋 Requirements

- .NET 8.0, 9.0, or 10.0
- ASP.NET Core (for DI extensions)

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

Built with ❤️ for the .NET community to make configuration management safer and more enjoyable.

---