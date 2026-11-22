using System.Globalization;
using FpvConfigurator.Catalog;
using FpvConfigurator.Services;
using FpvConfigurator.Web.Components;

// Числа і ціни в інтерфейсі показуємо в українському форматі (ФВ-24).
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("uk-UA");
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("uk-UA");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Корінь композиції (docs/DI.md): реєстр, фабрика, каталог, завантажувач, правила, BuildService.
builder.Services.AddFpvConfigurator();

var app = builder.Build();

// Стартовий каталог: файли зі списку Catalog:Files в appsettings.json.
var loader = app.Services.GetRequiredService<JsonCatalogLoader>();
foreach (var file in builder.Configuration.GetSection("Catalog:Files").Get<string[]>() ?? [])
{
    var path = Path.Combine(AppContext.BaseDirectory, file);
    if (File.Exists(path))
        loader.LoadFile(path);
    else
        app.Logger.LogWarning("Файл каталогу {Path} не знайдено", path);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();