using Microsoft.EntityFrameworkCore;
using app.Data;
using app.Services;
using app.Services.ParishService;
using app.Middleware;
using app.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Rejestracja usługi do wybierania i przechowywania aktualnie wybranej parafii (tenant'a)
builder.Services.AddScoped<ICurrentParishService, CurrentParishService>();

// Rejestracja kontekstów baz danych
builder.Services.RegisterDatabaseContexts(builder.Configuration).MigrateCentralDatabase();

// Rejestracja usługi do szyfrowania i deszyfrowania danych
builder.Services.AddSingleton<ICryptoService, CryptoService>();

// Rejestracja usługi do provisioningu baz danych parafii
builder.Services.AddTransient<IParishProvisioningService, ParishProvisioningService>();

// Dodanie usług do kontenera
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Migracja indywidualnych baz danych przy starcie aplikacji
using (var scope = app.Services.CreateScope())
{
    var prov = scope.ServiceProvider.GetRequiredService<IParishProvisioningService>();
    await prov.EnsureAllParishDatabasesReadyAsync();
}

// Konfiguracja pipelinu żądań HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
// Rejestracja middleware do czytania ciasteczka i ustawiania aktualnej parafii (tenant'a)
app.UseMiddleware<ParishResolver>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
