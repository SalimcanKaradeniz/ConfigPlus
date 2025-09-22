using ConfigPlus.Extensions;
using ConfigPlusExamples.Models;
using ConfigPlus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddConfigPlus(builder.Configuration);

if (builder.Environment.IsProduction())
{
    builder.Services.ConfigureFromConfigPlusForEnvironment<DatabaseSettings>("Database", "Production");
    builder.Services.ConfigureFromConfigPlusForEnvironment<EmailSettings>("Email", "Production");
}
else
{
    builder.Services.ConfigureFromConfigPlus<DatabaseSettings>("Database");
    builder.Services.ConfigureFromConfigPlus<EmailSettings>("Email");
}

var configSections = new Dictionary<string, Type>();

if (builder.Environment.IsProduction())
{
    configSections = new Dictionary<string, Type>
    {
        { "Database_Production", typeof(DatabaseSettings) },
        { "Email_Production", typeof(EmailSettings) }
    };
}
else
{
    configSections = new Dictionary<string, Type>
    {
        { "Database", typeof(DatabaseSettings) },
        { "Email", typeof(EmailSettings) }
    };
}

try
{
    builder.Services.ValidateConfigurations(configSections);
    Console.WriteLine("Tüm konfigürasyonlar baþarýyla doðrulandý!");
}
catch (AggregateException ex)
{
    Console.WriteLine("Konfigürasyon doðrulama hatasý:");
    foreach (var error in ex.InnerExceptions)
    {
        Console.WriteLine($"   • {error.Message}");
    }

    Console.WriteLine("Uygulama geçersiz konfigürasyon nedeniyle baþlatýlamýyor!");
    Environment.Exit(1);
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

Console.WriteLine($"ConfigPlusExamples {app.Environment.EnvironmentName} ortamýnda baþlatýlýyor...");

app.Run();
