using FastPin.Api.Auth;
using FastPin.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var appDataPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "FastPin");
Directory.CreateDirectory(appDataPath);
var dbPath = Path.Combine(appDataPath, "fastpin.db");

builder.Services.AddDbContext<FastPinApiDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

var authOptions = builder.Configuration
    .GetSection(ApiAuthOptions.SectionName)
    .Get<ApiAuthOptions>() ?? new ApiAuthOptions();

builder.Services.AddSingleton(authOptions);
builder.Services.AddSingleton<TokenStore>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FastPinApiDbContext>();
    DatabaseInitializer.Initialize(dbContext);
}

app.UseHttpsRedirection();
app.UseMiddleware<AuthMiddleware>();
app.MapControllers();
app.Run();
