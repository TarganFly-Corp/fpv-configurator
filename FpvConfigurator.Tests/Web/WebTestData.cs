using FpvConfigurator.Catalog;
using FpvConfigurator.Core;
using FpvConfigurator.Services;
using FpvConfigurator.Services.Rules.Custom;
using FpvConfigurator.Services.Storage;
using FpvConfigurator.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FpvConfigurator.Tests.Web;

/// <summary>
/// Тимчасова папка з даними користувача і ті самі сервіси, що й у Program.cs веб-застосунку.
/// Каталог заповнюється стартовими файлами catalog/*.json.
/// </summary>
internal sealed class WebTestData : IDisposable
{
    public WebTestData()
    {
        Directory.CreateDirectory(Folder);
        AddServices(Services);
    }

    public string Folder { get; } = Path.Combine(Path.GetTempPath(), "fpv-web-tests-" + Guid.NewGuid().ToString("N"));

    public string UserFile => Path.Combine(Folder, "user-catalog.json");

    public string RulesFile => Path.Combine(Folder, "rules.json");

    public ServiceCollection Services { get; } = new();

    public static string[] SeedFiles =>
    [
        Path.Combine(AppContext.BaseDirectory, "standard-parts.json"),
        Path.Combine(AppContext.BaseDirectory, "custom-parts.json")
    ];

    /// <summary>Реєструє сервіси веб-застосунку в будь-якій колекції (зокрема в контексті bUnit).</summary>
    public void AddServices(IServiceCollection services)
    {
        services.AddFpvConfigurator();
        services.AddSingleton<IBuildStorage>(_ => new JsonFileBuildStorage(Path.Combine(Folder, "builds.json")));
        services.AddSingleton<BuildLibrary>();
        services.AddSingleton(sp =>
        {
            var catalog = new UserCatalog(sp.GetRequiredService<ICategoryRegistry>(), sp.GetRequiredService<IComponentCatalog>(),
                                          sp.GetRequiredService<JsonCatalogLoader>(), UserFile);
            catalog.Initialize(SeedFiles);
            return catalog;
        });
        services.AddSingleton(sp =>
        {
            sp.GetRequiredService<UserCatalog>(); // як у Program.cs: спершу категорії користувача, потім правила
            var rules = new UserRules(sp.GetRequiredService<CustomRuleSet>(), RulesFile);
            rules.Initialize();
            return rules;
        });
        services.AddScoped<BuildSession>();
    }

    /// <summary>Новий «запуск» застосунку: свіжі сервіси, ті самі файли користувача.</summary>
    public UserCatalog StartApp(out IServiceProvider provider)
    {
        var services = new ServiceCollection();
        AddServices(services);
        provider = services.BuildServiceProvider();
        return provider.GetRequiredService<UserCatalog>();
    }

    public void Dispose()
    {
        if (Directory.Exists(Folder))
            Directory.Delete(Folder, recursive: true);
    }
}