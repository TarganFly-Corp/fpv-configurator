# FPV Configurator

Веб-конфігуратор FPV-дрона за принципом конфігуратора ПК: користувач обирає раму, польотний контролер, ESC, мотори, пропелери, камеру, відеопередавач і батарею. Система перевіряє сумісність компонентів і рахує загальну ціну та вагу збірки.

Проєкт модульний: окрім восьми стандартних категорій, будь-хто може описати власну категорію деталей зі своїми параметрами (наприклад, приймач або GPS-модуль) у файлі JSON. Приклад: catalog/custom-parts.json.

Репозиторій захищено: зміни в main лише через Pull Request з approve.

Стек: .NET 10, C#, Blazor, xUnit + Moq.

Документи: [Технічне завдання](docs/TZ.md), [План тестування](docs/TestPlan.md), [Правила роботи з історією Git](docs/GitWorkflow.md), [Ретроспектива ЛР3](docs/Retro-LR3.md), [Впровадження залежностей](docs/DI.md), [Макети в тестах](docs/Mocks.md).

Випуски: v0.2-lr2, v0.2.1 (виправлення завантажувача JSON, гілка release/0.2), v0.3-lr3, v0.3.1 (виправлення Motor, гілка release/0.3), v0.4-lr4, v0.5-lr5 (підписаний тег, архіви zip і tar.gz у GitHub Releases).

## Структура рішення

| Проєкт | Відповідальний | Призначення |
|---|---|---|
| FpvConfigurator.Core | Тімлід | Загальні інтерфейси і модель: IComponent, ParameterDefinition, ComponentCategory, ICategoryRegistry, IComponentFactory, ICompatibilityRule |
| FpvConfigurator.Components | Розробник | Стандартні класи деталей і CustomComponent |
| FpvConfigurator.Catalog | Розробник | Папки: Categories (схеми стандартних категорій, реєстр), Factory (фабрика деталей), Loading (завантажувач JSON), Search (каталог з пошуком і фільтрами ComponentCatalog, запит, сортування) |
| FpvConfigurator.Services | Розробник | Збірка Build, правила сумісності, сервіс BuildService, корінь композиції AddFpvConfigurator, папка Storage: бібліотека збірок BuildLibrary і інтерфейс сховища IBuildStorage (ФВ-15) |
| FpvConfigurator.Tests | Тестувальник | Модульні тести xUnit |
| FpvConfigurator.ManualCheck | Тестувальник | Консольна ручна перевірка |

## Команда і матриця відповідальності

| Роль | GitHub | ЛР2 | ЛР3 | ЛР4 | ЛР5 |
|---|---|---|---|---|---|
| Тімлід | targanfly | ТЗ, віхи, дошка Projects, Core, рев'ю і злиття PR | Правила роботи з історією Git, ретроспектива, випуск 0.2.1 | Вибір паттерна DI, аудит коду, завдання рефакторингу, гілки refactor/di і release/0.3, випуски 0.3.1 і 0.4 | Правила перегляду історії і обслуговування, .mailmap, export-ignore, перевірка підписів, виправлення нумерації ЛР, підписаний випуск 0.5, прибирання гілок |
| Розробник | SilentProg | Параметри стандартних деталей, CustomComponent, проєкт Catalog | ComponentCatalog, виправлення #29 і #30 | IComponentCatalog і впровадження в JsonCatalogLoader, проєкт Services, виправлення #43 | Папки в проєкті Catalog (git mv), CatalogSort в окремому файлі, історія через git log --follow і git blame -C |
| Тестувальник | SilentKroha | Проєкт Tests, план тестування, тести Core і Catalog, дефекти | Регресійні тести, тести каталогу, дефекти #29 і #30 | Тестовані класи і методи, тести незалежних класів без підміни, дефект #43 | Застарілі місця через git blame (#55), спільна папка TestData, приймання архіву і підписів випуску 0.5 |

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

## Гілки ЛР5

- main: захищена, основний стовбур
- docs/repo-maintenance, docs/lr5-summary: тімлід
- refactor/catalog-folders: розробник
- test/test-data-layout: тестувальник

Після злиття PR гілку видаляє тімлід. На GitHub постійно лишаються main, refactor/di і гілки підтримки release/*. Правила перегляду історії, прибирання і випуску описані в docs/GitWorkflow.md.


## Як додати власну деталь

1. Відкрити catalog/custom-parts.json.
2. У масив categories додати категорію: ключ, назву і список параметрів (type: Text, Integer, Number, Boolean або Choice; за потреби unit, min, max, options, required).
3. У масив components додати деталь з полем category, що дорівнює ключу категорії, і значеннями параметрів.
4. Завантажити файл через JsonCatalogLoader.LoadFile: деталі потраплять у каталог, який завантажувач отримав через конструктор.


## Пошук у каталозі

```csharp
var registry = CategoryRegistry.WithStandardCategories();
var catalog = new ComponentCatalog();
var loader = new JsonCatalogLoader(registry, ComponentFactory.WithStandardCreators(registry), catalog);
loader.LoadFile("catalog/custom-parts.json");
var receivers = catalog.Query(new CatalogQuery(CategoryKey: "Receiver", InStockOnly: true, Sort: CatalogSort.PriceAscending));
var part = catalog.FindById("RX-1"); // регістр не важливий
```

## Збірка дрона

Сервіси створює контейнер DI, кожен клас отримує залежності через конструктор (docs/DI.md).

```csharp
var provider = new ServiceCollection().AddFpvConfigurator().BuildServiceProvider();
provider.GetRequiredService<JsonCatalogLoader>().LoadFile("catalog/custom-parts.json");
var service = provider.GetRequiredService<BuildService>();

var build = new Build("5\" фрістайл");
service.AddPart(build, "fr-2");
var report = service.Check(build); // report.Problems, report.TotalPrice, report.TotalWeightGrams
```