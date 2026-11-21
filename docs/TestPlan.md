# План тестування: FPV Configurator

Версія 1.4 (ЛР6). Автор: тестувальник.

## 1. Що тестуємо

Модель параметрів і категорій (Core), стандартні і користувацькі деталі (Components), реєстр категорій, фабрику і JSON-завантажувач (Catalog). Основа для тестів: вимоги ФВ-01…ФВ-10 з docs/TZ.md.

## 2. Підхід

1. Модульні тести xUnit у проєкті FpvConfigurator.Tests. Кожен тест перевіряє одну поведінку і називається за схемою Метод_Умова_Результат.
2. Тести для Core пишуться одразу після злиття Core. Тести для Catalog пишуться на гілці, створеній від гілки розробника, щоб знайти дефекти до злиття в main.
3. Ручна перевірка в консольному проєкті ManualCheck: завантаження прикладу catalog/custom-parts.json і вивід характеристик.
4. Знайдений дефект оформлюється як Issue з міткою bug, картка в Projects переходить у статус «Помилка».
5. У ЛР4 тестуються лише незалежні класи і без підміни: тест створює справжні об'єкти Frame, Motor, Battery тощо.
6. Межові значення перевіряються парами: значення на межі схеми категорії приймається, сусіднє за межею відхиляється.
7. З ЛР6 залежні класи тестуються з макетами Moq замість справжніх залежностей (docs/Mocks.md). Однакові за схемою тести об'єднуються в один [Theory] з набором вхідних даних.

## 3. Середовище

Windows 11, .NET SDK 10, Visual Studio 2026, xUnit 2.9, Moq 4.21. Запуск усіх тестів: dotnet test FpvConfigurator.slnx.

## 4. Критерії

Початок тестування: проєкт збирається без помилок. Завершення: усі тест-кейси з пріоритетом «високий» пройдено, відкритих дефектів з міткою bug немає.

## 5. Тест-кейси

| ID | Вимога | Опис | Вхідні дані | Очікуваний результат | Пріоритет |
|---|---|---|---|---|---|
| TC-01 | ФВ-03 | Обов'язковий параметр не задано | Integer «KV», значення null | Повертається текст помилки | високий |
| TC-02 | ФВ-03 | Необов'язковий параметр не задано | Boolean «Телеметрія», required: false, null | Помилки немає | середній |
| TC-03 | ФВ-04 | Межі цілого числа | min 1, max 4; значення 0, 1, 4, 5 | 1 і 4 приймаються, 0 і 5 ні | високий |
| TC-04 | ФВ-04 | Дробове значення для цілого параметра | Integer, значення 2.5 | Помилка «має бути цілим числом» | середній |
| TC-05 | ФВ-03 | Дробовий параметр приймає int і double | Number від 0.8 до 2.5; значення 2.4 і 2 | Обидва приймаються | середній |
| TC-06 | ФВ-04 | Значення не зі списку | Choice [ELRS, Crossfire]; значення PPM | Помилка | високий |
| TC-07 | ФВ-03 | Choice без варіантів | new ParameterDefinition(..., Choice) | ArgumentException | середній |
| TC-08 | ФВ-03 | Мінімум більший за максимум | min 5, max 1 | ArgumentException | низький |
| TC-09 | ФВ-04 | Категорія: коректні значення | GPS: chip, hasCompass | Список помилок порожній | високий |
| TC-10 | ФВ-04 | Категорія: пропущений параметр | GPS без hasCompass | Одна помилка | високий |
| TC-11 | ФВ-04 | Категорія: зайвий параметр | GPS + color | Помилка з назвою color | середній |
| TC-12 | ФВ-03 | Два параметри з однаковим ключем | a, a | ArgumentException | низький |
| TC-13 | ФВ-01 | Стандартні категорії в реєстрі | WithStandardCategories() | 8 категорій, усі IsStandard | високий |
| TC-14 | ФВ-05 | Пошук власної категорії без урахування регістру | Register(Gps), TryGet("gps") | Знайдено, IsStandard = false | високий |
| TC-15 | ФВ-05 | Повторний ключ категорії | Register(Frame) | InvalidOperationException | середній |
| TC-16 | ФВ-05 | Невідома категорія | Get("Gimbal") | KeyNotFoundException | середній |
| TC-17 | ФВ-10 | Фабрика створює стандартний клас | Frame з параметрами | Об'єкт типу Frame | високий |
| TC-18 | ФВ-06, ФВ-09 | Фабрика створює користувацьку деталь | Gps, hasCompass = true | CustomComponent, Type = Custom, «Компас: так» | високий |
| TC-19 | ФВ-04 | Фабрика відхиляє неправильні параметри | Frame, stackMount 16x16 | ArgumentException | високий |
| TC-20 | ФВ-03 | Параметри стандартних класів відповідають схемам | 8 об'єктів | Жодної помилки валідації | високий |
| TC-21 | ФВ-07 | JSON: власна категорія і деталь | Receiver + rx-1 | Одна деталь CustomComponent, antennas = 1 | високий |
| TC-22 | ФВ-07 | JSON: значення поза межами | antennas = 9 | InvalidDataException з Id rx-1 | високий |
| TC-23 | ФВ-08 | JSON: повторний Id деталі | rx-1 двічі | InvalidDataException | високий |
| TC-24 | ФВ-07 | Приклад каталогу | catalog/custom-parts.json | 3 деталі, fr-2 має тип Frame | середній |
| TC-25 | ФВ-08 | JSON: повторний Id в іншому регістрі (дефект #29) | rx-1 і RX-1 | InvalidDataException | високий |
| TC-26 | ФВ-07 | JSON: деталь невідомої категорії (дефект #30) | category: Gimbal | InvalidDataException з Id rx-1 і назвою категорії | високий |
| TC-27 | ФВ-25 | Каталог: пошук за Id в іншому регістрі | FR-1 | Знайдено fr-1 | високий |
| TC-28 | ФВ-25 | Каталог: повторний Id в іншому регістрі | MT-1 при наявному mt-1 | InvalidOperationException, кількість не змінилась | високий |
| TC-29 | ФВ-26 | Каталог: фільтр за категорією | motor | Лише mt-1 | високий |
| TC-30 | ФВ-26 | Каталог: лише в наявності | InStockOnly | Деталі без gps-1 | середній |
| TC-31 | ФВ-26 | Каталог: пошук без урахування регістру | tbs | fr-1 (виробник TBS) | високий |
| TC-32 | ФВ-26 | Каталог: пошук у назві і виробнику | 1950KV, CNHL | mt-1, bt-1 | середній |
| TC-33 | ФВ-27 | Каталог: сортування за ціною за спаданням | PriceDescending | bt-1, fr-1, mt-1, gps-1 | середній |
| TC-34 | ФВ-07, ФВ-26 | Каталог з файлу | catalog/custom-parts.json | 3 деталі, фільтр gps дає gps-1 | середній |
| TC-35 | ФВ-03 | Батарея: межі кількості банок | 1S, 8S; 0S, 9S | 1S і 8S приймаються і проходять схему, 0S і 9S дають ArgumentException | високий |
| TC-36 | ФВ-03 | Пропелер: межі кількості лопатей | 2, 6; 1, 7 | 2 і 6 приймаються, 1 і 7 дають ArgumentException | середній |
| TC-37 | ФВ-03 | Контролер і ESC: діапазон банок | 1…12S, 3…6S; 0…6, 6…4, 3…13 | Діапазони в межах 1…12S приймаються, решта дає ArgumentException | високий |
| TC-38 | ФВ-03 | Камера: ширина | 14, 19; 16 мм | 14 і 19 приймаються, 16 дає ArgumentException | середній |
| TC-39 | ФВ-03 | Відеопередавач: діапазон частот | 1.2, 5.8; 5.0 ГГц | 1.2 і 5.8 приймаються, 5.0 дає ArgumentException | середній |
| TC-40 | ФВ-03 | Мотор: діапазон банок і струм (дефект #43) | 1…1, 1…12, 4…6; 0…6, 6…4, 4…13; струм −5 A | Діапазони в межах 1…12S приймаються, решта і від'ємний струм дають ArgumentException | високий |
| TC-41 | ФВ-11 | Збірка: повний набір деталей | рама, контролер, ESC, 4 мотори, 4 пропелери, камера, VTX, батарея | 14 деталей у збірці | високий |
| TC-42 | ФВ-11 | Збірка: п'ятий мотор | 4 мотори + ще один | InvalidOperationException з назвою категорії, моторів 4 | високий |
| TC-43 | ФВ-11 | Збірка: друга рама | 2 рами | InvalidOperationException | високий |
| TC-44 | ФВ-11 | Збірка: користувацькі деталі без обмеження | 5 GPS-модулів | 5 деталей у збірці | середній |
| TC-45 | ФВ-11 | Збірка: видалення за Id | MT-1 при 4 моторах mt-1; невідомий Id | Лишається 3 мотори; для невідомого Id повертається false | високий |
| TC-46 | ФВ-14 | Збірка: ціна і вага | рама 1200 грн 110 г + батарея 1500 грн 210 г; порожня збірка | 2700 грн і 320 г; 0 і 0 | високий |
| TC-47 | ФВ-12 | Кріплення стеку: усе збігається | рама, контролер, ESC 4-в-1 30.5x30.5 | Проблем немає | високий |
| TC-48 | ФВ-12 | Кріплення стеку: контролер 20x20 на рамі 30.5x30.5 | | Одна проблема з назвою контролера | високий |
| TC-49 | ФВ-12 | Кріплення стеку: ESC 4-в-1 іншого розміру; окремий ESC | ESC 20x20 при контролері 30.5x30.5; ESC не 4-в-1 | Одна проблема; для окремого ESC проблем немає | середній |
| TC-50 | ФВ-12 | Пропелер більший за раму | 4 пропелери 7" на рамі 5.1" | Одна проблема (однакові пропелери не дублюють повідомлення) | високий |
| TC-51 | ФВ-12 | Банки батареї в діапазоні всіх деталей | 6S, мотори 4…6S, ESC і контролер 3…6S | Проблем немає | високий |
| TC-52 | ФВ-12 | Батарея поза діапазоном | 6S, мотор 2…4S, ESC 2…4S | Дві проблеми з назвами мотора і ESC | високий |
| TC-53 | ФВ-12 | Відеосистема камери і VTX | Analog + DJI; DJI + DJI | Одна проблема; для однакових проблем немає | високий |
| TC-54 | ФВ-12 | Неповна і повна сумісна збірка | лише рама; повний набір з 14 деталей | Жодне з чотирьох правил не повертає проблем | середній |
| TC-55 | ФВ-11 | Збірка з порожньою назвою | " " | ArgumentException | низький |

## 6. ЛР4: тестовані класи і методи

Класи поділено за тим, чи залежать вони від інших сервісів проєкту (див. docs/DI.md). У ЛР4 тестуються лише незалежні.

| Клас | Проєкт | Залежності | Вид | Що тестуємо | Тест-кейси |
|---|---|---|---|---|---|
| ParameterDefinition | Core | немає | незалежний | Validate | TC-01…TC-08 (ЛР2) |
| ComponentCategory | Core | немає | незалежний | конструктор, Validate | TC-09…TC-12 (ЛР2) |
| Frame, FlightController, Esc, Motor, Propeller, Camera, Vtx, Battery | Components | немає | незалежні | конструктори на межах, Parameters відповідає схемі | TC-20, TC-35…TC-40 |
| CategoryRegistry | Catalog | немає | незалежний | Register, TryGet, Get | TC-13…TC-16 (ЛР2) |
| ComponentCatalog | Catalog | немає | незалежний | Add, FindById, Query | TC-27…TC-33 (ЛР3) |
| Build | Services | немає | незалежний | конструктор, Add, Remove, CountOf, LimitFor, TotalPrice, TotalWeightGrams | TC-41…TC-46, TC-55 |
| StackMountRule, PropellerSizeRule, CellCountRule, VideoSystemRule | Services | немає | незалежні | Check | TC-47…TC-54 |
| ComponentFactory | Catalog | ICategoryRegistry | залежний | Create | ЛР6 (Moq) |
| JsonCatalogLoader | Catalog | ICategoryRegistry, IComponentFactory, IComponentCatalog | залежний | Load | ЛР6 (Moq) |
| BuildService | Services | IComponentCatalog, ICompatibilityRule | залежний | AddPart, Check | ЛР6 (Moq) |

## 7. Звіт про тестування ЛР4

Прогін: dotnet test на гілці test/independent-classes після злиття refactor/di (PR #47). Моків і підробних класів у тестах немає, кожен тест створює справжні об'єкти.

| Набір | Файл | Тестів | Результат |
|---|---|---|---|
| Межові значення деталей (TC-35…TC-39) | Components/ComponentBoundaryTests.cs | 22 | пройдено |
| Мотор (TC-40, дефект #43) | Components/MotorTests.cs | 7 | до виправлення 2 провалено, після v0.3.1 пройдено |
| Збірка (TC-41…TC-46, TC-55) | Services/BuildTests.cs | 8 | пройдено |
| Правила сумісності (TC-47…TC-54) | Services/CompatibilityRulesTests.cs | 12 | пройдено |
| Тести ЛР2 і ЛР3 | Core/*, Catalog/* | 39 | пройдено |
| Разом | | 88 | 88 пройдено, 0 провалено |

Знайдений дефект: #43, Motor приймав діапазон банок понад 12S і від'ємний струм, хоча схема категорії це забороняє. Виправлено у версії 0.3.1 (гілка release/0.3) і перенесено в main та refactor/di.

Ручна перевірка ManualCheck: 20 PASS, 0 FAIL, серед них збірка через контейнер DI і одна проблема відеосистеми для камери Analog з VTX DJI.

## 8. ЛР5: перевірка випуску 0.5

У ЛР5 нових вимог немає. Код Catalog розкладено по папках, допоміжний клас тестів TestParts перенесено в папку TestData, тому всі 88 тестів мають проходити без змін. Перед випуском тестувальник перевіряє сам архів, який отримає користувач: розпаковує його в окрему папку поза репозиторієм і збирає там рішення.

| ID | Що перевіряємо | Як | Очікуваний результат |
|---|---|---|---|
| TC-56 | Вміст архіву | tar -tf архів.zip | є шість проєктів, catalog, docs, README.md і FpvConfigurator.slnx; немає .git, bin, obj, .gitattributes, .gitignore, .mailmap |
| TC-57 | Збірка з архіву | dotnet build у розпакованій папці | 0 помилок |
| TC-58 | Тести з архіву | dotnet test у розпакованій папці | 88 пройдено, 0 провалено |
| TC-59 | Ручна перевірка з архіву | dotnet run --project FpvConfigurator.ManualCheck | 20 PASS, 0 FAIL |
| TC-60 | Підписи | git verify-commit для коміту злиття, git verify-tag v0.5-lr5 | Good signature: коміт підписано ключем GitHub, тег ключем тімліда |

## 9. ЛР6: методи для тестування з макетами і набори вхідних даних

### 9.1 Тести ЛР4, перероблені на набори даних

| Було | Стало | Набір вхідних даних |
|---|---|---|
| 4 окремі тести StackMount_* | StackMount_Check_ReturnsExpectedProblemCount | 5 комбінацій кріплення рами, контролера і ESC, ESC 4-в-1 або окремий; очікувано 0, 1 або 2 проблеми |
| 2 тести PropellerSize_* | PropellerSize_FourPropsOnFrame51_ReturnsExpectedProblemCount | пропелер 3.0, 5.1 (межа), 5.2, 7.0" на рамі 5.1" |
| 2 тести CellCount_* | CellCount_Battery_ReturnsExpectedProblemCount і перевірка назв деталей у тексті проблем | батарея 6S, 4S (нижня межа мотора), 3S, 6S проти 2…4S |
| VideoSystem з 2 наборами | той самий тест, 4 набори | додано Analog+Analog і HDZero+Walksnail |
| 2 тести AllRules_* | AllRules_CompatibleBuild_NoProblems | лише рама; повний набір з 14 деталей |
| Add_FifthMotor, Add_SecondFrame | Add_OverCategoryLimit_ThrowsAndKeepsLimit | усі 8 стандартних категорій з їхнім лімітом (1 або 4) |
| Constructor_EmptyName_Throws | той самий тест, 3 набори | "", " ", табуляція |

### 9.2 Залежні класи: методи і набори даних

| ID | Клас і метод | Макети | Вхідні дані | Очікуваний результат |
|---|---|---|---|---|
| TC-61 | BuildService.AddPart | IComponentCatalog | mt-1, MT-1 | деталь з каталогу у збірці, FindById викликано один раз |
| TC-62 | BuildService.AddPart | IComponentCatalog | mt-404, порожній Id | KeyNotFoundException з Id, збірка порожня |
| TC-63 | BuildService.AddPart | IComponentCatalog | п'ятий мотор у збірці з 4 моторами | InvalidOperationException, моторів 4 |
| TC-64 | BuildService.Check | ICompatibilityRule, IComponentCatalog | кількість проблем по правилах: [], [0,0], [1,0], [2,3], [0,0,0,1] | 0, 0, 1, 5, 1 проблем; кожне правило викликано один раз; каталог не викликано |
| TC-65 | BuildService.Check | ICompatibilityRule | одне правило з однією проблемою; рама і батарея | проблема з назвою правила, 2700 грн, 320 г |
| TC-66 | JsonCatalogLoader.Load | ICategoryRegistry, IComponentFactory, IComponentCatalog | 1, 3, 5 деталей у файлі | Add викликано стільки ж разів, Register жодного |
| TC-67 | JsonCatalogLoader.Load | ті самі | фабрика відхиляє деталь: 1 з 1, 1-шу з 3, 3-тю з 3 | InvalidDataException з Id, Add жодного разу |
| TC-68 | JsonCatalogLoader.Load | ті самі | у каталозі вже є fr-1 або fr-3 | InvalidDataException з Id, Add жодного разу |
| TC-69 | JsonCatalogLoader.Load | ті самі | файл лише з категорією Receiver | Register один раз, фабрика і каталог не викликані |
| TC-70 | ComponentFactory.Create | ICategoryRegistry | категорія Gimbal, осей 1 і 3 | CustomComponent |
| TC-71 | ComponentFactory.Create | ICategoryRegistry | осей 0 і 4 | ArgumentException з назвою категорії |
| TC-72 | ComponentFactory.Create | ICategoryRegistry | реєстр кидає KeyNotFoundException для Gimbal, frame | виняток не перехоплюється |
| TC-73 | ComponentFactory.RegisterCreator | ICategoryRegistry | власний «конструктор» для Gimbal | Create повертає його деталь |
| TC-74 | BuildLibrary.Save | IBuildStorage | порожня збірка; рама і мотор; повний набір | у сховище записано назву і Id у порядку додавання |
| TC-75 | BuildLibrary.Save | IBuildStorage | назва зайнята, overwrite false і true | false: InvalidOperationException, Save не викликано; true: Save один раз |
| TC-76 | BuildLibrary.Open | IBuildStorage, IComponentCatalog | збірки з 3, 1 і 0 деталей | деталі з каталогу в тому самому порядку |
| TC-77 | BuildLibrary.Open | IBuildStorage | невідома назва | KeyNotFoundException |
| TC-78 | BuildLibrary.Names | IBuildStorage | [b, A, c]; [Racer, freestyle, Cine]; [] | за абеткою без урахування регістру |
| TC-79 | BuildLibrary.Open | IBuildStorage, IComponentCatalog | у збірці деталь, якої немає в каталозі: [fr-1, mt-404], [gone-1], [fr-1, mt-1, pr-9, mt-1] | InvalidDataException з Id деталі і назвою збірки (дефект #69) |

### 9.3 Класи-обгортки

Повторюване налаштування макетів винесено в FpvConfigurator.Tests/Mocks/Wrappers.

| Обгортка | Що підміняє | Навіщо |
|---|---|---|
| CatalogMock | IComponentCatalog | каталог з переданих деталей; FindById і Contains без урахування регістру; VerifyAdded, VerifyLookups |
| RuleMock | ICompatibilityRule | правило з назвою і готовим списком проблем; набір правил за кількістю проблем |
| StorageMock | IBuildStorage | сховище з переданими збірками; усе, що передали в Save, лежить у Written |
| LoaderUnderTest | ICategoryRegistry, IComponentFactory, IComponentCatalog | JsonCatalogLoader з трьома макетами; FactoryRejects і CatalogHas для наборів з помилками |

## 10. Звіт про тестування ЛР6

Прогін: dotnet test на гілці test/moq-tests після злиття PR #68 (макети від розробника) і PR #70 (виправлення #69).

| Набір | Файл | Тестів | Результат |
|---|---|---|---|
| BuildService з макетами (TC-61…TC-65) | Mocks/BuildServiceMockTests.cs | 15 | пройдено |
| JsonCatalogLoader з макетами (TC-66…TC-69) | Mocks/JsonCatalogLoaderMockTests.cs | 12 | пройдено |
| ComponentFactory з макетом реєстру (TC-70…TC-73) | Mocks/ComponentFactoryMockTests.cs | 7 | пройдено |
| BuildLibrary з макетами (TC-74…TC-79) | Mocks/BuildLibraryMockTests.cs | 16 | до виправлення 3 провалено (TC-79), після #70 пройдено |
| Тести ЛР4, перероблені на набори даних | Services/BuildTests.cs, Services/CompatibilityRulesTests.cs | 36 (було 20) | пройдено |
| Тести ЛР2…ЛР4 без змін | Core/*, Catalog/*, Components/* | 68 | пройдено |
| Разом | | 154 | 154 пройдено, 0 провалено, 0 пропущено |

Знайдений дефект: #69. BuildLibrary.Open мовчки пропускав деталі, яких уже немає в каталозі, і відкривав неповну збірку без жодного повідомлення. Знайдено набором даних TC-79, де макет каталогу не знає одного з Id. Виправлено в PR #70: Open кидає InvalidDataException з назвою збірки і Id деталі.

Ручна перевірка ManualCheck: 20 PASS, 0 FAIL, без змін після ЛР5.