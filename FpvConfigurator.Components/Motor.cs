using FpvConfigurator.Core;

namespace FpvConfigurator.Components;

public class Motor : ComponentBase
{
    public Motor(string id, string name, string manufacturer, decimal price, double weightGrams,
                 string statorSize, int kv, int minCells, int maxCells, double maxCurrentA)
        : base(id, name, manufacturer, price, weightGrams)
    {
        if (kv <= 0)
            throw new ArgumentOutOfRangeException(nameof(kv), "KV має бути більшим за 0");
        if (minCells < 1 || maxCells < minCells)
            throw new ArgumentException("Некоректний діапазон батареї (S)");

        StatorSize = statorSize;
        Kv = kv;
        MinCells = minCells;
        MaxCells = maxCells;
        MaxCurrentA = maxCurrentA;
    }

    public string StatorSize { get; }
    public int Kv { get; }
    public int MinCells { get; }
    public int MaxCells { get; }
    public double MaxCurrentA { get; }

    public override ComponentType Type => ComponentType.Motor;

    public override IReadOnlyDictionary<string, string> GetSpecs() => new Dictionary<string, string>
    {
        ["Статор"] = StatorSize,
        ["KV"] = Kv.ToString(),
        ["Батарея"] = $"{MinCells}–{MaxCells}S",
        ["Макс. струм"] = $"{MaxCurrentA} A"
    };
}
