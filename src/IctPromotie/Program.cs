using IctPromotie.Data;
using IctPromotie.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Database: standaard buiten OneDrive/Git, in de lokale app-data van de gebruiker.
// Overschrijven kan met ConnectionStrings:Promotie in appsettings.json.
var verbinding = builder.Configuration.GetConnectionString("Promotie");
if (string.IsNullOrWhiteSpace(verbinding))
{
    var map = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IctPromotie");
    verbinding = $"Data Source={Path.Combine(map, "promotie.db")}";
}
var csb = new SqliteConnectionStringBuilder(verbinding);
if (!Path.IsPathRooted(csb.DataSource))
    csb.DataSource = Path.Combine(builder.Environment.ContentRootPath, csb.DataSource);
Directory.CreateDirectory(Path.GetDirectoryName(csb.DataSource)!);
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(csb.ToString()));

// Nieuws via Google Nieuws.
builder.Services.Configure<NieuwsOpties>(builder.Configuration.GetSection("Nieuws"));
builder.Services.AddHttpClient<NieuwsOphaler>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(20);
    c.DefaultRequestHeaders.UserAgent.ParseAdd("IctPromotie/1.0");
});
builder.Services.AddHostedService<NieuwsAchtergrondDienst>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (app.Configuration.GetValue("VoorbeelddataToevoegen", true)) Voorbeelddata.Vul(db);
    app.Logger.LogInformation("Database: {Pad}", csb.DataSource);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRequestLocalization("nl-NL");
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();
