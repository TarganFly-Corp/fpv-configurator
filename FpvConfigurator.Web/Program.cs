using System.Globalization;
using System.Text;
using FpvConfigurator.Catalog;
using FpvConfigurator.Core;
using FpvConfigurator.Services;
using FpvConfigurator.Services.Rules.Custom;
using FpvConfigurator.Services.Storage;
using FpvConfigurator.Web.Components;
using FpvConfigurator.Web.Services;

// Числа, ціни і дати в інтерфейсі показуємо в українському форматі (ФВ-24).
CultureInfo.DefaultThreadCurrentCulture = PartFormat.Culture;
CultureInfo.DefaultThreadCurrentUICulture = PartFormat.Culture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Корінь композиції (docs/DI.md): реєстр, фабрика, каталог, завантажувач, правила, BuildService.
builder.Services.AddFpvConfigurator();

// Дані користувача: збережені збірки, власні категорії, деталі і правила (ФВ-20, ФВ-23, ФВ-28).
var dataFolder = Path.Combine(builder.Environment.ContentRootPath,
                              builder.Configuration["Storage:DataFolder"] ?? "App_Data");
builder.Services.AddSingleton<IBuildStorage>(_ => new JsonFileBuildStorage(Path.Combine(dataFolder, "builds.json")));
builder.Services.AddSingleton<BuildLibrary>();
builder.Services.AddSingleton(sp => new UserCatalog(
    sp.GetRequiredService<ICategoryRegistry>(),
    sp.GetRequiredService<IComponentCatalog>(),
    sp.GetRequiredService<JsonCatalogLoader>(),
    Path.Combine(dataFolder, "user-catalog.json")));
builder.Services.AddSingleton(sp => new UserRules(
    sp.GetRequiredService<CustomRuleSet>(),
    Path.Combine(dataFolder, "rules.json")));

// Поточна збірка: окрема для кожної вкладки браузера.
builder.Services.AddScoped<BuildSession>();

var app = builder.Build();

// Стартовий каталог з appsettings.json, потім деталі користувача, потім правила користувача
// (правила можуть посилатися на категорії користувача, тому завантажуються останніми).
var seedFiles = builder.Configuration.GetSection("Catalog:Files").Get<string[]>() ?? [];
app.Services.GetRequiredService<UserCatalog>()
   .Initialize(seedFiles.Select(f => Path.Combine(AppContext.BaseDirectory, f)));
app.Services.GetRequiredService<UserRules>().Initialize();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

// Експорт каталогу у файл JSON (ФВ-23): /api/catalog/export або /api/catalog/export?scope=user
app.MapGet("/api/catalog/export", (UserCatalog catalog, string? scope) =>
{
    var userOnly = string.Equals(scope, "user", StringComparison.OrdinalIgnoreCase);
    var json = catalog.Export(userOnly);
    return Results.File(Encoding.UTF8.GetBytes(json), "application/json",
                        userOnly ? "fpv-my-parts.json" : "fpv-catalog.json");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

/// <summary>Потрібен для інтеграційних тестів (WebApplicationFactory).</summary>
public partial class Program;