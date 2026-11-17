# FPV Configurator

Веб-конфігуратор FPV-дрона за принципом конфігуратора ПК: користувач обирає раму, польотний контролер, ESC, мотори, пропелери, камеру, відеопередавач і батарею. Система перевіряє сумісність компонентів і рахує загальну ціну та вагу збірки.

Проєкт модульний: окрім восьми стандартних категорій, будь-хто може описати власну категорію деталей зі своїми параметрами (наприклад, приймач або GPS-модуль) у файлі JSON. Приклад: catalog/custom-parts.json.

Репозиторій захищено: зміни в main лише через Pull Request з approve.

Стек: .NET 10, C#, Blazor, xUnit + Moq.

Документи: [Технічне завдання](docs/TZ.md), [План тестування](docs/TestPlan.md), [Правила роботи з історією Git](docs/GitWorkflow.md), [Ретроспектива ЛР3](docs/Retro-LR3.md), [Впровадження залежностей](docs/DI.md).

Випуски: v0.2-lr2, v0.2.1 (виправлення завантажувача JSON, гілка release/0.2), v0.3-lr3.

## Структура рішення

| Проєкт | Відповідальний | Призначення |
|---|---|---|
| FpvConfigurator.Core | Тімлід | Загальні інтерфейси і модель: IComponent, ParameterDefinition, ComponentCategory, ICategoryRegistry, IComponentFactory, ICompatibilityRule |
| FpvConfigurator.Components | Розробник | Стандартні класи деталей і CustomComponent |
| FpvConfigurator.Catalog | Розробник | Схеми стандартних категорій, реєстр, фабрика, завантажувач JSON, каталог з пошуком і фільтрами (ComponentCatalog) |
| FpvConfigurator.Tests | Тестувальник | Модульні тести xUnit |
| FpvConfigurator.ManualCheck | Тестувальник | Консольна ручна перевірка |

## Команда і матриця відповідальності

| Роль | GitHub | ЛР2 | ЛР3 |
|---|---|---|---|
| Тімлід | targanfly | ТЗ, віхи, дошка Projects, Core, рев'ю і злиття PR | Правила роботи з історією Git, ретроспектива, випуск 0.2.1 |
| Розробник | SilentProg | Параметри стандартних деталей, CustomComponent, проєкт Catalog | ComponentCatalog, виправлення #29 і #30 |
| Тестувальник | SilentKroha | Проєкт Tests, план тестування, тести Core і Catalog, дефекти | Регресійні тести, тести каталогу, дефекти #29 і #30 |

## Гілки ЛР3

- main: захищена
- release/0.2: випуск 0.2.1, лише cherry-pick виправлень
- docs/git-workflow, docs/lr3-retro: тімлід
- fix/catalog-loader, feature/component-catalog: розробник
- test/loader-regression, test/component-catalog: тестувальник

Правила роботи з гілками і історією описані в docs/GitWorkflow.md.

## Гілки ЛР4

- main: захищена, основний стовбур
- refactor/di: гілка ітерації ЛР4, захищена, сюди зливаються всі задачі рефакторингу
- release/0.3: підтримка версії 0.3, лише виправлення (випуск 0.3.1)
- refactor/catalog-injection, feature/build-service, hotfix/motor-limits: розробник
- test/independent-classes: тестувальник

Паттерн впровадження залежностей, аудит коду і завдання рефакторингу описані в docs/DI.md.

## Як додати власну деталь

1. Відкрити catalog/custom-parts.json.
2. У масив categories додати категорію: ключ, назву і список параметрів (type: Text, Integer, Number, Boolean або Choice; за потреби unit, min, max, options, required).
3. У масив components додати деталь з полем category, що дорівнює ключу категорії, і значеннями параметрів.
4. Завантажити файл через JsonCatalogLoader.LoadFile.


## Пошук у каталозі

```csharp
var catalog = loader.LoadCatalogFile("catalog/custom-parts.json");
var receivers = catalog.Query(new CatalogQuery(CategoryKey: "Receiver", InStockOnly: true, Sort: CatalogSort.PriceAscending));
var part = catalog.FindById("RX-1"); // регістр не важливий
```