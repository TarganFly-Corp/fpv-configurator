# Покриття коду модульними тестами

Документ веде тімлід (#64). Тут описано, як команда вимірює покриття, які були результати до і після ЛР6 і що з цього випливає для наступної віхи.

## 1. Інструменти

| Інструмент | Версія | Що робить |
|---|---|---|
| coverlet.collector | 6.0.4 | під час dotnet test записує, які рядки і розгалуження коду виконувались; результат у форматі Cobertura (coverage.cobertura.xml). Пакет є в FpvConfigurator.Tests з ЛР2 |
| ReportGenerator | 5.6.0 | з файлу Cobertura робить HTML-звіт з підсвіченими рядками і коротке зведення Summary.txt. Ставиться один раз як глобальний інструмент dotnet |

Збірка тестів (FpvConfigurator.Tests) і ManualCheck у звіт не входять: coverlet рахує лише бібліотеки, які тести викликають.

## 2. Як виміряти (cmd, корінь репозиторію)

```cmd
REM один раз на комп'ютері
dotnet tool install -g dotnet-reportgenerator-globaltool

REM прогін тестів зі збором покриття
rmdir /s /q coverage coveragereport
dotnet test --collect:"XPlat Code Coverage" --results-directory coverage

REM звіт HTML і зведення
reportgenerator -reports:"coverage\**\coverage.cobertura.xml" -targetdir:coveragereport -reporttypes:"Html;TextSummary"
type coveragereport\Summary.txt
start coveragereport\index.html
```

Папки coverage і coveragereport є в .gitignore: результати не комітимо, у документ переносимо лише цифри.

## 3. Результати

Порівняно два стани: тег v0.5-lr5 (88 тестів, без макетів) і main після злиття PR #67 (154 тести, з макетами).

| Показник | v0.5-lr5 | після ЛР6 |
|---|---|---|
| Тестів | 88 | 154 |
| Покриття рядків (Line coverage) | 82% (672 з 819) | 86.1% (728 з 845) |
| Покриття розгалужень (Branch coverage) | 76.3% (242 з 317) | 78.2% (259 з 331) |
| Покриття методів (Method coverage) | 85.4% (164 з 192) | 91.8% (181 з 197) |

За проєктами і класами, що змінилися:

| Проєкт або клас | v0.5-lr5 | після ЛР6 | Чому змінилося |
|---|---|---|---|
| FpvConfigurator.Services | 64.3% | 88.3% | тести з макетами для BuildService і BuildLibrary |
| BuildService | 0% | 95.2% | у ЛР4 залежний клас не тестувався, тепер TC-61…TC-65 |
| BuildLibrary, SavedBuild | класів не було | 100% | TC-74…TC-79 |
| BuildReport | 0% | 100% | його створює BuildService.Check |
| Правила сумісності | 83.3…93.3% | 91.6…100% | набори даних замість окремих тестів (межа 5.1" пропелера, 4S батареї) |
| FpvConfigurator.Catalog | 97.6% | 98% | JsonCatalogLoader і ComponentFactory і раніше тестувались на справжніх об'єктах, макети додали перевірку взаємодії, а не нові рядки |
| FpvConfigurator.Core | 90.5% | 90.5% | без змін |
| FpvConfigurator.Components | 74.7% | 75% | без змін |

## 4. Що лишилося непокритим

1. ServiceCollectionExtensions (0%). Корінь композиції перевіряє лише ManualCheck, а його coverlet не рахує. Задача на M6: тест, що контейнер створює BuildService і BuildLibrary.
2. Components (75%). Непокриті здебільшого GetSpecs і ToString деталей: текст характеристик, який з'явиться в інтерфейсі. Тести на нього має сенс писати разом зі сторінками Blazor (M6).
3. IComponent (0%). Це рядки властивостей за замовчуванням в інтерфейсі, усі стандартні деталі їх перевизначають.
4. BuildProblem (50%): не викликано ToString.

Висновок для команди: залежні класи з макетами тепер покриті на рівні незалежних. Ціль на M6 не менше 85% рядків для Services і Catalog, перевіряємо перед кожним випуском.