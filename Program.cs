using Serilog;
using System.Text;
using UserManagementAPI;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddSingleton<UserManagementAPI.Services.UserService>();

builder.Services.AddOpenApiDocument(settings =>
{
    settings.Title = "User Management API";
    settings.Version = "v1";
    settings.Description = "API for managing users in the system.";
    settings.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter JWT token"
    });
    settings.OperationProcessors.Add(new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("Bearer"));
});

var app = builder.Build();

app.UseExceptionHandling();
app.UseOpenApi();     
app.UseSwaggerUi();   
app.UseHttpsRedirection();
app.UseRequestLogging();
app.UseTokenAuthentication();

app.MapControllers();

app.Run();