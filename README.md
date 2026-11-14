# FPV Configurator

Веб-конфігуратор FPV-дрона за принципом конфігуратора ПК: користувач обирає раму, польотний контролер, ESC, мотори, пропелери, камеру, відеопередавач і батарею. Система перевіряє сумісність компонентів і рахує загальну ціну та вагу збірки.

Проєкт модульний: окрім восьми стандартних категорій, будь-хто може описати власну категорію деталей зі своїми параметрами (наприклад, приймач або GPS-модуль) у файлі JSON. Приклад: catalog/custom-parts.json.

Репозиторій захищено: зміни в main лише через Pull Request з approve.

Стек: .NET 10, C#, Blazor, xUnit + Moq.

Документи: [Технічне завдання](docs/TZ.md), [План тестування](docs/TestPlan.md).

## Структура рішення

| Проєкт | Відповідальний | Призначення |
|---|---|---|
| FpvConfigurator.Core | Тімлід | Загальні інтерфейси і модель: IComponent, ParameterDefinition, ComponentCategory, ICategoryRegistry, IComponentFactory, ICompatibilityRule |
| FpvConfigurator.Components | Розробник | Стандартні класи деталей і CustomComponent |
| FpvConfigurator.Catalog | Розробник | Схеми стандартних категорій, реєстр, фабрика, завантажувач JSON |
| FpvConfigurator.Tests | Тестувальник | Модульні тести xUnit |
| FpvConfigurator.ManualCheck | Тестувальник | Консольна ручна перевірка |

## Команда і матриця відповідальності

| Роль | GitHub | ЛР2 |
|---|---|---|
| Тімлід | targanfly | ТЗ, віхи, дошка Projects, Core, рев'ю і злиття PR |
| Розробник | SilentProg | Параметри стандартних деталей, CustomComponent, проєкт Catalog |
| Тестувальник | SilentKroha | Проєкт Tests, план тестування, тести Core і Catalog, дефекти |

## Гілки ЛР2

- main: захищена
- docs/tz-lr2: тімлід (ТЗ, README)
- feature/core-modular: тімлід (Core)
- feature/modular-catalog: розробник
- test/unit-tests, test/catalog-tests: тестувальник

## Як додати власну деталь

1. Відкрити catalog/custom-parts.json.
2. У масив categories додати категорію: ключ, назву і список параметрів (type: Text, Integer, Number, Boolean або Choice; за потреби unit, min, max, options, required).
3. У масив components додати деталь з полем category, що дорівнює ключу категорії, і значеннями параметрів.
4. Завантажити файл через JsonCatalogLoader.LoadFile.