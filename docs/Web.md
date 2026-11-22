# Веб-застосунок FpvConfigurator.Web (віха M6)

Документ описує, як влаштовано веб-застосунок, які сторінки закривають вимоги ФВ-16…ФВ-24 і ФВ-28 і як його запустити. Автор: тімлід.

## 1. Запуск

```cmd
cd FpvConfigurator.Web
dotnet run --launch-profile http
```

Адресу показує рядок «Now listening on: http://localhost:NNNN». У Visual Studio: FpvConfigurator.Web → Set as Startup Project → F5. Зупинити: Ctrl+C у вікні cmd.

## 2. Рішення

1. Шаблон Blazor Web App з режимом Interactive Server для всього застосунку (--all-interactive). Код сторінок виконується на сервері, браузер отримує зміни через SignalR. Тому сторінки працюють з тими самими класами Core, Catalog і Services напряму, без окремого Web API і без копій моделей у браузері (ФВ-16).
2. Залежності приходять через впровадження залежностей. Program.cs викликає AddFpvConfigurator() з проєкту Services (docs/DI.md) і додає лише те, що потрібно вебу: справжнє сховище збірок, бібліотеку збірок, каталог користувача, правила користувача і поточну збірку.
5. Правила сумісності користувача (ФВ-28) зроблено як ще одне правило для BuildService: клас CustomRuleSet реалізує ICompatibilityRule і зареєстрований в AddFpvConfigurator поруч з чотирма вбудованими. Редактор додає в нього описи правил, тому BuildService не змінюється і не перестворюється, а нове правило діє з наступної перевірки збірки.
3. Сторінки нічого не перевіряють самі. Сумісність перевіряє BuildService з правилами, параметри перевіряє схема категорії (ParameterDefinition.Validate) і фабрика, збереження робить BuildLibrary.
4. Бібліотек стилів немає: один файл wwwroot/app.css. Макет перевірено на ширині 360 px (ФВ-24).

## 3. Сервіси

| Сервіс | Час життя | Хто дає | Призначення |
|---|---|---|---|
| ICategoryRegistry, IComponentFactory, IComponentCatalog, JsonCatalogLoader, правила, BuildService | Singleton | AddFpvConfigurator | Модель і логіка з ЛР2…ЛР6 |
| IBuildStorage → JsonFileBuildStorage | Singleton | Program.cs | Збірки у файлі App_Data/builds.json (ФВ-20) |
| BuildLibrary | Singleton | Program.cs | Зберегти, відкрити, перейменувати, видалити збірку |
| UserCatalog | Singleton | Program.cs | Стартовий каталог, власні категорії і деталі, імпорт і експорт (ФВ-21…ФВ-23) |
| CustomRuleSet | Singleton | AddFpvConfigurator | Правила користувача, п'яте правило BuildService (ФВ-28) |
| UserRules | Singleton | Program.cs | Додати і видалити правило, файл App_Data/rules.json (ФВ-28) |
| BuildSession | Scoped | Program.cs | Поточна збірка окремо для кожної вкладки браузера (ФВ-19) |

## 4. Дані

| Файл | Що в ньому | У Git |
|---|---|---|
| catalog/standard-parts.json | 24 деталі восьми стандартних категорій | так |
| catalog/custom-parts.json | приклад користувацьких категорій (приймач, GPS) і три деталі | так |
| FpvConfigurator.Web/App_Data/user-catalog.json | категорії і деталі, додані через інтерфейс або імпортом | ні (.gitignore) |
| FpvConfigurator.Web/App_Data/builds.json | збережені збірки: назва і Id деталей | ні (.gitignore) |
| FpvConfigurator.Web/App_Data/rules.json | правила сумісності користувача | ні (.gitignore) |

Список стартових файлів задає appsettings.json (Catalog:Files), папку даних користувача Storage:DataFolder.

## 5. Сторінки і вимоги

| Адреса | Сторінка | Вимоги |
|---|---|---|
| / | Головна: кількість категорій, деталей, збірок | ФВ-16 |
| /catalog | Каталог: фільтр за категорією, пошук, сортування, наявність, кнопка «У збірку» | ФВ-17 |
| /catalog/{id} | Деталь: загальні дані і характеристики за схемою категорії | ФВ-18 |
| /build | Конструктор: слоти стандартних категорій, додаткові деталі, проблеми сумісності, ціна, вага, збереження | ФВ-19, ФВ-20 |
| /builds | Мої збірки: відкрити, перейменувати, видалити | ФВ-20 |
| /categories | Схеми всіх категорій і редактор нової категорії | ФВ-21 |
| /rules | Вбудовані правила сумісності і редактор правил користувача | ФВ-28 |
| /parts/new | Форма нової деталі за схемою категорії | ФВ-22 |
| /data | Імпорт JSON і експорт (/api/catalog/export) | ФВ-23 |

## 6. Хто що робить у M6

| Роль | Задача | Гілка |
|---|---|---|
| Тімлід | #72 каркас застосунку і цей документ, #78 випуск 1.0 | chore/web-skeleton, docs/v1.0 |
| Розробник | #73 сховище і експорт, #74 каталог і збірка, #75 редактори та імпорт, #77 редактор правил | feature/web-backend, feature/web-pages, feature/web-editors, feature/rule-editor |
| Тестувальник | #76 модульні тести, тести сторінок bUnit, ручна перевірка, звіт | test/web |