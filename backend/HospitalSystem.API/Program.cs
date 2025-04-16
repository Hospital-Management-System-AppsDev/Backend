using DotNetEnv;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ✅ Load environment variables from .env file
Env.Load();

// ✅ Read database configuration from environment variables
string connectionString = $"Server={Environment.GetEnvironmentVariable("MYSQL_SERVER")};" +
                          $"Port={Environment.GetEnvironmentVariable("MYSQL_PORT")};" +
                          $"Database={Environment.GetEnvironmentVariable("MYSQL_DATABASE")};" +
                          $"User Id={Environment.GetEnvironmentVariable("MYSQL_USER")};" +
                          $"Password={Environment.GetEnvironmentVariable("MYSQL_PASSWORD")};";

// ✅ Register DatabaseConnection as a singleton service
builder.Services.AddSingleton(new DatabaseConnection(connectionString));

// ✅ Register SignalR
builder.Services.AddSignalR();

// ✅ Add controllers and enable CORS
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// ✅ Apply middleware in correct order
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

// ✅ Map SignalR Hub
app.MapHub<HospitalHub>("/hospitalHub"); // Ensure DoctorHub.cs exists

app.Run();
