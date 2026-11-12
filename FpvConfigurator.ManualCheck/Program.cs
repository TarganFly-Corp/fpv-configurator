using FpvConfigurator.Components;
using FpvConfigurator.Core;
using System.ComponentModel;

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

Console.WriteLine($"Разом: {passed} PASS, {failed} FAIL");
return failed == 0 ? 0 : 1;
