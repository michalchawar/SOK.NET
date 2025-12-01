using Microsoft.AspNetCore.Identity;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Implementation;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Central;
using SOK.Infrastructure.Extensions;
using SOK.Infrastructure.Identity;
using SOK.Infrastructure.Persistence.Context;
using SOK.Infrastructure.Provisioning;
using SOK.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Rejestracja usługi do szyfrowania i deszyfrowania danych
builder.Services.AddSingleton<ICryptoService, CryptoService>();

// Rejestracja usługi do wybierania i przechowywania aktualnie wybranej parafii (tenant'a)
builder.Services.AddScoped<ICurrentParishService, CurrentParishService>();

// Rejestracja kontekstów baz danych
builder.Services.RegisterDatabaseContexts(builder.Configuration).MigrateCentralDatabase();

// Rejestracja repozytoriów
builder.Services.RegisterRepositories();

// Rejestracja usług danych
builder.Services.RegisterServices();

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

// Rejestracja fabryki do tworzenia obiektu ClaimsPrincipal z dodatkowymi danymi
builder.Services.AddScoped<IUserClaimsPrincipalFactory<User>, AppClaimsPrincipalFactory>();

// Rejestracja usługi do provisioningu baz danych parafii
builder.Services.AddTransient<IParishProvisioningService, ParishProvisioningService>();

// Rejestracja middleware do rozpoznawania parafii (tenant'a)
builder.Services.AddTransient<ParishResolver>();

// Dodanie usług do kontenera
builder.Services.AddControllersWithViews();

// Konfiguracja WebOptimizer
builder.Services.AddWebOptimizer(pipeline =>
{
    // Bundle i minifikuj JavaScript
    pipeline.AddJavaScriptBundle("/js/bundle.min.js", "js/site.js", "js/address-utilities.js");
});

var app = builder.Build();

// Migracja indywidualnych baz danych przy starcie aplikacji
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var prov = services.GetRequiredService<IParishProvisioningService>();
    await prov.EnsureAllParishDatabasesReadyAsync();

    var seeder = services.GetRequiredService<IDbSeeder>();
    await seeder.SeedAsync();
}

// Konfiguracja pipelinu żądań HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// WebOptimizer musi być przed UseStaticFiles
app.UseWebOptimizer();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Rejestracja middleware do czytania ciasteczka i ustawiania aktualnej parafii (tenant'a)
app.UseMiddleware<ParishResolver>();

app.MapControllerRoute(
    name: "publicForm",
    pattern: "{parishUid}/submissions/{action=New}",
    defaults: new { controller = "PublicForm" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
