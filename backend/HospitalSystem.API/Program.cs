using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// ✅ Load environment variables from .env file
Env.Load();

// ✅ Build the connection string dynamically from environment variables
string connectionString = $"Server={Environment.GetEnvironmentVariable("MYSQL_SERVER")};" +
                          $"Port={Environment.GetEnvironmentVariable("MYSQL_PORT")};" +
                          $"Database={Environment.GetEnvironmentVariable("MYSQL_DATABASE")};" +
                          $"User Id={Environment.GetEnvironmentVariable("MYSQL_USER")};" +
                          $"Password={Environment.GetEnvironmentVariable("MYSQL_PASSWORD")};";

// ✅ Register DatabaseConnection with the resolved connection string
builder.Services.AddSingleton(new DatabaseConnection(connectionString));

builder.Services.AddControllers();

var app = builder.Build();

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
