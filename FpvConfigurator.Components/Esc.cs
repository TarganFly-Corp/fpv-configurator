using FpvConfigurator.Core;

namespace FpvConfigurator.Components;

public class Esc : ComponentBase
{
    private static readonly string[] StackMounts = { "20x20", "25.5x25.5", "30.5x30.5" };
    private static readonly string[] Protocols = { "DShot150", "DShot300", "DShot600", "DShot1200" };

    public Esc(string id, string name, string manufacturer, decimal price, double weightGrams,
               int continuousCurrentA, int minCells, int maxCells,
               bool isFourInOne, string stackMount, string protocol)
        : base(id, name, manufacturer, price, weightGrams)
    {
        if (continuousCurrentA <= 0)
            throw new ArgumentOutOfRangeException(nameof(continuousCurrentA), "Струм має бути більшим за 0");
        if (minCells < 1 || maxCells > 12 || minCells > maxCells)
            throw new ArgumentOutOfRangeException(nameof(minCells), "Діапазон банок має бути в межах 1–12S і min ≤ max");
        if (isFourInOne && !StackMounts.Contains(stackMount))
            throw new ArgumentException($"Невідоме кріплення стеку: {stackMount}", nameof(stackMount));
        if (!Protocols.Contains(protocol))
            throw new ArgumentException($"Невідомий протокол: {protocol}", nameof(protocol));

        ContinuousCurrentA = continuousCurrentA;
        MinCells = minCells;
        MaxCells = maxCells;
        IsFourInOne = isFourInOne;
        StackMount = isFourInOne ? stackMount : "—";
        Protocol = protocol;
    }

    public int ContinuousCurrentA { get; }
    public int MinCells { get; }
    public int MaxCells { get; }
    public bool IsFourInOne { get; }
    public string StackMount { get; }
    public string Protocol { get; }

    public override ComponentType Type => ComponentType.Esc;

    public override IReadOnlyDictionary<string, string> GetSpecs() => new Dictionary<string, string>
    {
        ["Струм"] = $"{ContinuousCurrentA} A",
        ["Живлення"] = $"{MinCells}–{MaxCells}S",
        ["Тип"] = IsFourInOne ? "4-в-1" : "Окремий",
        ["Кріплення стеку"] = StackMount,
        ["Протокол"] = Protocol
    };
}
