using Microsoft.EntityFrameworkCore;
using app.Data;
using app.Services;

var builder = WebApplication.CreateBuilder(args);

// Rejestracja usługi ParishService do obsługi wyboru parafii (tenant'ów)
builder.Services.AddScoped<IParishGetter, ParishService>();
builder.Services.AddScoped<IParishSetter, ParishService>();

// Rejestracja usługi CryptoService do szyfrowania i deszyfrowania danych
builder.Services.AddSingleton<ICryptoService, CryptoService>();

// Rejestracja usługi do provisioningu baz danych dla parafii
builder.Services.AddScoped<IProvisioningService, ProvisioningService>();


// Rejestracja konfiguracji parafii (tenant'ów) z pliku konfiguracyjnego
builder.Services.Configure<ParishConfigurationSection>(
    builder.Configuration.GetSection("Parishes"));


// Rejestracja kontekstu bazy danych dla centralnej bazy danych
builder.Services.AddDbContext<CentralDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CentralDbContext") ?? throw new InvalidOperationException("Connection string 'CentralDbContext' not found.")));


// Rejestracja middleware do obsługi wielu parafii (tenant'ów)
builder.Services.AddScoped<MultiParishServiceMiddleware>();
// Rejestracja kontekstu bazy danych dla indywidualnych parafii (tenant'ów)
builder.Services.AddSingleton<IParishDbContextFactory, ParishDbContextFactory>();

// Rejestracja kontekstu bazy danych dla indywidualnych baz danych parafii
builder.Services.AddDbContext<ParishDbContext>((services, options) =>
{
    var parish = services.GetService<IParishGetter>()?.Parish;

    //var connectionString = parish?.ConnectionString ?? throw new InvalidOperationException(string.Format("Connection string for parish with UID '{uid}' not found.", parish?.UniqueId));
    var connectionString = builder.Configuration.GetRequiredSection("Parishes").GetChildren().ToList().FirstOrDefault()?["ConnectionString"] ?? throw new InvalidOperationException(string.Format("Connection string for parish with UID '{uid}' not found.", parish?.UniqueId));
    options.UseSqlServer(connectionString);
});

// Dodanie usług do kontenera
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var central = scope.ServiceProvider.GetRequiredService<CentralDbContext>();
    await central.Database.MigrateAsync();

    // Dociągnięcie migracji do wszystkich istniejących parafii
    var prov = scope.ServiceProvider.GetRequiredService<IProvisioningService>();
    await prov.EnsureAllParishDatabasesMigratedAsync();
}

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
