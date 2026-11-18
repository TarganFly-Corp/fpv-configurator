using FpvConfigurator.Catalog;
using FpvConfigurator.Core;
using FpvConfigurator.Services.Rules;
using Microsoft.Extensions.DependencyInjection;

namespace FpvConfigurator.Services;

/// <summary>
/// Корінь композиції: тут вирішується, яка реалізація стоїть за кожним інтерфейсом.
/// Класи лише оголошують залежності в конструкторі, контейнер передає їх сам.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFpvConfigurator(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ICategoryRegistry>(_ => CategoryRegistry.WithStandardCategories());
        services.AddSingleton<IComponentFactory>(sp =>
            ComponentFactory.WithStandardCreators(sp.GetRequiredService<ICategoryRegistry>()));
        services.AddSingleton<IComponentCatalog>(_ => new ComponentCatalog());
        services.AddSingleton<JsonCatalogLoader>();

        // Нове правило сумісності (ФВ-13) достатньо дописати сюди одним рядком.
        services.AddSingleton<ICompatibilityRule, StackMountRule>();
        services.AddSingleton<ICompatibilityRule, PropellerSizeRule>();
        services.AddSingleton<ICompatibilityRule, CellCountRule>();
        services.AddSingleton<ICompatibilityRule, VideoSystemRule>();

        services.AddSingleton<BuildService>();
        return services;
    }
}