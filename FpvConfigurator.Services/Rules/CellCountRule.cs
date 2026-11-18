using FpvConfigurator.Components;
using FpvConfigurator.Core;

namespace FpvConfigurator.Services.Rules;

/// <summary>Кількість банок батареї входить у діапазон кожного мотора, ESC і контролера (ФВ-12).</summary>
public sealed class CellCountRule : ICompatibilityRule
{
    public string Name => "Живлення";

    public IEnumerable<string> Check(IReadOnlyCollection<IComponent> build)
    {
        ArgumentNullException.ThrowIfNull(build);
        var battery = build.OfType<Battery>().FirstOrDefault();
        if (battery is null)
            return [];

        var ranges = build.OfType<Motor>().Select(m => (m.Id, Kind: "мотора", m.Name, m.MinCells, m.MaxCells))
            .Concat(build.OfType<Esc>().Select(e => (e.Id, Kind: "ESC", e.Name, e.MinCells, e.MaxCells)))
            .Concat(build.OfType<FlightController>().Select(f => (f.Id, Kind: "контролера", f.Name, f.MinCells, f.MaxCells)))
            .DistinctBy(r => r.Id, StringComparer.OrdinalIgnoreCase);

        return ranges
            .Where(r => battery.Cells < r.MinCells || battery.Cells > r.MaxCells)
            .Select(r => $"батарея {battery.Cells}S не підходить для {r.Kind} {r.Name} ({r.MinCells}–{r.MaxCells}S)")
            .ToList();
    }
}