using FpvConfigurator.Core;

namespace FpvConfigurator.Components;

public class Battery : ComponentBase
{
    public Battery(string id, string name, string manufacturer, decimal price, double weightGrams,
                   int cells, int capacityMah, int cRating, string connector)
        : base(id, name, manufacturer, price, weightGrams)
    {
        if (cells is < 1 or > 8)
            throw new ArgumentOutOfRangeException(nameof(cells), "Кількість банок має бути від 1 до 8");
        if (capacityMah <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacityMah), "Ємність має бути більшою за 0");

        Cells = cells;
        CapacityMah = capacityMah;
        CRating = cRating;
        Connector = connector;
    }

    public int Cells { get; }
    public int CapacityMah { get; }
    public int CRating { get; }
    public string Connector { get; }
    public double NominalVoltage => Cells * 3.7;

    public override ComponentType Type => ComponentType.Battery;

    public override IReadOnlyDictionary<string, string> GetSpecs() => new Dictionary<string, string>
    {
        ["Банок"] = $"{Cells}S ({NominalVoltage:0.0} В)",
        ["Ємність"] = $"{CapacityMah} мАг",
        ["Струмвіддача"] = $"{CRating}C",
        ["Роз'єм"] = Connector
    };
}
