using FpvConfigurator.Catalog;
using FpvConfigurator.Components;
using FpvConfigurator.Core;
using System.ComponentModel;
using FpvConfigurator.Services;
using Microsoft.Extensions.DependencyInjection;

Console.OutputEncoding = System.Text.Encoding.UTF8;
int passed = 0, failed = 0;

void Check(string name, bool condition)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")}  {name}");
    if (condition) passed++; else failed++;
}

void ExpectThrows<T>(string name, Action action) where T : Exception
{
    try { action(); Check(name, false); }
    catch (T) { Check(name, true); }
    catch (Exception ex) { Console.WriteLine($"      неочікуваний виняток {ex.GetType().Name}"); Check(name, false); }
}

// 1. Коректні об'єкти і взаємодія через інтерфейс
FpvConfigurator.Core.IComponent[] parts =
[
    new Frame("fr-1", "Source One V5", "TBS", 1200m, 110, 5.1, "30.5x30.5", 225),
    new Motor("mt-1", "2207 1950KV", "T-Motor", 900m, 33, "2207", 1950, 4, 6, 40),
    new Propeller("pr-1", "51466", "Gemfan", 120m, 4.2, 5.1, 4.66, 3),
    new Battery("bt-1", "1300mAh 6S", "CNHL", 1500m, 210, 6, 1300, 100, "XT60")
];

foreach (var p in parts)
{
    Console.WriteLine(p);
    foreach (var (key, value) in p.GetSpecs())
        Console.WriteLine($"      {key}: {value}");
}

Check("Усі класи повертають правильний тип",
    parts.Select(p => p.Type).SequenceEqual(
        new[] { ComponentType.Frame, ComponentType.Motor, ComponentType.Propeller, ComponentType.Battery }));
Check("У кожної деталі є характеристики", parts.All(p => p.GetSpecs().Count > 0));
Check("Загальна ціна рахується через інтерфейс", parts.Sum(p => p.Price) == 3720m);
Check("Напруга 6S = 22.2 В", Math.Abs(((Battery)parts[3]).NominalVoltage - 22.2) < 0.001);

// 2. Обробка помилок
ExpectThrows<ArgumentException>("Порожній Id",
    () => new Frame("", "X", "Y", 100m, 100, 5, "30.5x30.5", 220));
ExpectThrows<ArgumentOutOfRangeException>("Від'ємна ціна",
    () => new Motor("m", "X", "Y", -1m, 30, "2207", 1950, 4, 6, 40));
ExpectThrows<ArgumentOutOfRangeException>("Нульова вага",
    () => new Propeller("p", "X", "Y", 100m, 0, 5, 4, 3));
ExpectThrows<ArgumentException>("Невідоме кріплення стеку 16x16",
    () => new Frame("f", "X", "Y", 100m, 100, 5, "16x16", 220));
ExpectThrows<ArgumentException>("Мотор: minCells > maxCells",
    () => new Motor("m", "X", "Y", 100m, 30, "2207", 1950, 6, 4, 40));
ExpectThrows<ArgumentOutOfRangeException>("Гвинт з однією лопаттю",
    () => new Propeller("p", "X", "Y", 100m, 4, 5, 4, 1));
ExpectThrows<ArgumentOutOfRangeException>("Батарея 0S",
    () => new Battery("b", "X", "Y", 100m, 200, 0, 1300, 100, "XT60"));
ExpectThrows<ArgumentOutOfRangeException>("Рама з від'ємною колісною базою",
    () => new Frame("f", "X", "Y", 100m, 100, 5, "30.5x30.5", -5));

// 3. Користувацькі деталі з JSON (ЛР2)
var registry = CategoryRegistry.WithStandardCategories();
var catalog = new ComponentCatalog();
var loader = new JsonCatalogLoader(registry, ComponentFactory.WithStandardCreators(registry), catalog);
var custom = loader.LoadFile(Path.Combine(AppContext.BaseDirectory, "custom-parts.json"));

foreach (var p in custom)
{
    Console.WriteLine(p);
    foreach (var (key, value) in p.GetSpecs())
        Console.WriteLine($"      {key}: {value}");
}

Check("З JSON завантажено 3 деталі", custom.Count == 3);
Check("Приймач став користувацькою деталлю", custom.Any(p => p.Type == ComponentType.Custom && p.CategoryKey == "Receiver"));
Check("Рама з JSON створена як клас Frame", custom.Any(p => p is Frame));
Check("Категорій у реєстрі стало 10", registry.All.Count == 10);
Check("Деталі з JSON потрапили в каталог", catalog.Count == 3 && catalog.Contains("RX-1"));

// 4. Збірка через контейнер DI (ЛР4): об'єкти створює контейнер, класи отримують залежності через конструктор
var provider = new ServiceCollection().AddFpvConfigurator().BuildServiceProvider();
var appCatalog = provider.GetRequiredService<IComponentCatalog>();
var buildService = provider.GetRequiredService<BuildService>();

provider.GetRequiredService<JsonCatalogLoader>().LoadFile(Path.Combine(AppContext.BaseDirectory, "custom-parts.json"));
foreach (var p in parts)
    appCatalog.Add(p);
appCatalog.Add(new FlightController("fc-1", "F722 HD", "SpeedyBee", 1900m, 9, "STM32F722", "30.5x30.5", 3, 6, "Betaflight", 5));
appCatalog.Add(new Esc("esc-1", "BLS 50A 4in1", "SpeedyBee", 2200m, 14, 50, 3, 6, true, "30.5x30.5", "DShot600"));
appCatalog.Add(new Camera("cam-1", "Ratel 2", "Caddx", 1100m, 5.9, "Analog", 19, "1200TVL", 165));
appCatalog.Add(new Vtx("vtx-1", "O3 Air Unit", "DJI", 4500m, 36, "DJI", 1000, 5.8, "20x20"));

var build = new Build("5\" фрістайл");
foreach (var id in new[] { "fr-1", "fc-1", "esc-1", "mt-1", "mt-1", "mt-1", "mt-1", "pr-1", "pr-1", "pr-1", "pr-1", "bt-1", "rx-1" })
    buildService.AddPart(build, id);
var report = buildService.Check(build);
Console.WriteLine($"{build.Name}: {build.Parts.Count} деталей, {report.TotalPrice} грн, {report.TotalWeightGrams:0.#} г");

Check("Контейнер передав сервісу 4 правила", buildService.Rules.Count == 4);
Check("Збірка без камери і VTX сумісна", report.IsCompatible);

buildService.AddPart(build, "cam-1");
buildService.AddPart(build, "vtx-1");
report = buildService.Check(build);
foreach (var problem in report.Problems)
    Console.WriteLine($"      {problem}");
Check("Камера Analog і VTX DJI: одна проблема відеосистеми", report.Problems.Count == 1 && report.Problems[0].Rule == "Відеосистема");

Console.WriteLine($"Разом: {passed} PASS, {failed} FAIL");
return failed == 0 ? 0 : 1;
