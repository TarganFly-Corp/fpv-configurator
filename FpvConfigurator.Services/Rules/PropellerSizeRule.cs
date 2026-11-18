using FpvConfigurator.Components;
using FpvConfigurator.Core;

namespace FpvConfigurator.Services.Rules;

/// <summary>Діаметр пропелера не більший за допустимий для рами (ФВ-12).</summary>
public sealed class PropellerSizeRule : ICompatibilityRule
{
    public string Name => "Розмір пропелерів";

    public IEnumerable<string> Check(IReadOnlyCollection<IComponent> build)
    {
        ArgumentNullException.ThrowIfNull(build);
        var frame = build.OfType<Frame>().FirstOrDefault();
        if (frame is null)
            return [];

        return build.OfType<Propeller>()
            .DistinctBy(p => p.Id, StringComparer.OrdinalIgnoreCase)
            .Where(p => p.DiameterInches > frame.MaxPropInches)
            .Select(p => $"пропелер {p.Name} ({p.DiameterInches}\") більший за допустимий для рами {frame.Name} ({frame.MaxPropInches}\")")
            .ToList();
    }
}