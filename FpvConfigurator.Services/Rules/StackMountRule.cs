using FpvConfigurator.Components;
using FpvConfigurator.Core;

namespace FpvConfigurator.Services.Rules;

/// <summary>Кріплення стеку рами, польотного контролера і ESC 4-в-1 мають збігатися (ФВ-12).</summary>
public sealed class StackMountRule : ICompatibilityRule
{
    public string Name => "Кріплення стеку";

    public IEnumerable<string> Check(IReadOnlyCollection<IComponent> build)
    {
        ArgumentNullException.ThrowIfNull(build);
        var problems = new List<string>();
        var frame = build.OfType<Frame>().FirstOrDefault();
        var fc = build.OfType<FlightController>().FirstOrDefault();
        var esc = build.OfType<Esc>().FirstOrDefault(e => e.IsFourInOne);

        if (frame is not null && fc is not null && fc.StackMount != frame.StackMount)
            problems.Add($"контролер {fc.Name} ({fc.StackMount}) не стає на раму {frame.Name} ({frame.StackMount})");

        if (esc is not null && fc is not null && esc.StackMount != fc.StackMount)
            problems.Add($"ESC {esc.Name} ({esc.StackMount}) не збігається з контролером {fc.Name} ({fc.StackMount})");
        else if (esc is not null && fc is null && frame is not null && esc.StackMount != frame.StackMount)
            problems.Add($"ESC {esc.Name} ({esc.StackMount}) не стає на раму {frame.Name} ({frame.StackMount})");

        return problems;
    }
}