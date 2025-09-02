using app.Data;
using app.Data.Seeding;
using app.Extensions;
using app.Middleware;
using app.Models.Central.Entities;
using app.Services;
using app.Services.Parish;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Rejestracja usługi do wybierania i przechowywania aktualnie wybranej parafii (tenant'a)
builder.Services.AddScoped<ICurrentParishService, CurrentParishService>();

// Rejestracja kontekstów baz danych
builder.Services.RegisterDatabaseContexts(builder.Configuration).MigrateCentralDatabase();

builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireUppercase = false;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 4;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<CentralDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(6);
    });


// Rejestracja usługi do szyfrowania i deszyfrowania danych
builder.Services.AddSingleton<ICryptoService, CryptoService>();

// Rejestracja usługi do provisioningu baz danych parafii
builder.Services.AddTransient<IParishProvisioningService, ParishProvisioningService>();

// Rejestracja middleware do rozpoznawania parafii (tenant'a)
builder.Services.AddTransient<ParishResolver>();

// Dodanie usług do kontenera
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Migracja indywidualnych baz danych przy starcie aplikacji
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var prov = services.GetRequiredService<IParishProvisioningService>();
    await prov.EnsureAllParishDatabasesReadyAsync();

    var context = services.GetRequiredService<CentralDbContext>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await CentralDbSeeder.SeedAsync(context, userManager, roleManager, prov);
}

// Konfiguracja pipelinu żądań HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Rejestracja middleware do czytania ciasteczka i ustawiania aktualnej parafii (tenant'a)
app.UseMiddleware<ParishResolver>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
