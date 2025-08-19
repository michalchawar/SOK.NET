using Microsoft.EntityFrameworkCore;
using app.Data;
using app.Services;

var builder = WebApplication.CreateBuilder(args);

// Rejestracja usługi IParishGetter i IParishSetter
builder.Services.AddScoped<IParishGetter, ParishService>();
builder.Services.AddScoped<IParishSetter, ParishService>();

// Rejestracja konfiguracji parafii (tenant'ów) z pliku konfiguracyjnego
builder.Services.Configure<ParishConfigurationSection>(
    builder.Configuration.GetSection("Parishes"));

// Rejestracja kontekstu bazy danych dla centralnej bazy danych
builder.Services.AddDbContext<centralDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("centralDbContext") ?? throw new InvalidOperationException("Connection string 'centralDbContext' not found.")));

// Rejestracja middleware do obsługi wielu parafii (tenant'ów)
builder.Services.AddScoped<MultiParishServiceMiddleware>();

// Rejestracja kontekstu bazy danych dla indywidualnych parafii (tenant'ów)
builder.Services.AddDbContext<sokAppContext>((services, options) =>
{
    var parish = services.GetService<IParishGetter>()?.Parish;

    var connectionString = parish?.ConnectionString ?? throw new InvalidOperationException(string.Format("Connection string for parish with UID '{uid}' not found.", parish?.UniqueId));
    options.UseSqlServer(connectionString);
});

// Dodanie usług do kontenera
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Rejestracja middleware do czytania ciasteczka i ustawiania aktualnej parafii (tenant'a)
app.UseMiddleware<MultiParishServiceMiddleware>();

// Konfiguracja pipelinu żądań HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
