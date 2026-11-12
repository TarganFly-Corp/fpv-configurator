using FpvConfigurator.Core;

namespace FpvConfigurator.Components;

public class Frame : ComponentBase
{
    public Frame(string id, string name, string manufacturer, decimal price, double weightGrams,
                 double maxPropInches, string stackMount, int wheelbaseMm)
        : base(id, name, manufacturer, price, weightGrams)
    {
        if (maxPropInches <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxPropInches), "Розмір гвинта має бути більшим за 0");
        if (stackMount is not ("20x20" or "25.5x25.5" or "30.5x30.5"))
            throw new ArgumentException($"Невідоме кріплення стеку: {stackMount}", nameof(stackMount));
        if (wheelbaseMm <= 0)
            throw new ArgumentOutOfRangeException(nameof(wheelbaseMm), "Колісна база має бути більшою за 0 (wheelbaseMm > 0)");

        MaxPropInches = maxPropInches;
        StackMount = stackMount;
        WheelbaseMm = wheelbaseMm;
    }

    public double MaxPropInches { get; }
    public string StackMount { get; }
    public int WheelbaseMm { get; }

    public override ComponentType Type => ComponentType.Frame;

    public override IReadOnlyDictionary<string, string> GetSpecs() => new Dictionary<string, string>
    {
        ["Макс. гвинт"] = $"{MaxPropInches}\"",
        ["Кріплення стеку"] = StackMount,
        ["Колісна база"] = $"{WheelbaseMm} мм"
    };
}
