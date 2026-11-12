using FpvConfigurator.Core;

namespace FpvConfigurator.Components;

public class Propeller : ComponentBase
{
    public Propeller(string id, string name, string manufacturer, decimal price, double weightGrams,
                     double diameterInches, double pitch, int blades)
        : base(id, name, manufacturer, price, weightGrams)
    {
        if (diameterInches <= 0)
            throw new ArgumentOutOfRangeException(nameof(diameterInches), "Діаметр має бути більшим за 0");
        if (blades is < 2 or > 6)
            throw new ArgumentOutOfRangeException(nameof(blades), "Кількість лопатей має бути від 2 до 6");

        DiameterInches = diameterInches;
        Pitch = pitch;
        Blades = blades;
    }

    public double DiameterInches { get; }
    public double Pitch { get; }
    public int Blades { get; }

    public override ComponentType Type => ComponentType.Propeller;

    public override IReadOnlyDictionary<string, string> GetSpecs() => new Dictionary<string, string>
    {
        ["Діаметр"] = $"{DiameterInches}\"",
        ["Крок"] = Pitch.ToString(),
        ["Лопатей"] = Blades.ToString()
    };
}
