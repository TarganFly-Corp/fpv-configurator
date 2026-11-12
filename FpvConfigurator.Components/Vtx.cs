using FpvConfigurator.Core;

namespace FpvConfigurator.Components;

public class Vtx : ComponentBase
{
    private static readonly string[] VideoSystems = { "Analog", "DJI", "Walksnail", "HDZero" };
    private static readonly string[] Mounts = { "20x20", "25.5x25.5", "30.5x30.5" };

    public Vtx(string id, string name, string manufacturer, decimal price, double weightGrams,
               string videoSystem, int maxPowerMw, double frequencyGhz, string mount)
        : base(id, name, manufacturer, price, weightGrams)
    {
        if (!VideoSystems.Contains(videoSystem))
            throw new ArgumentException($"Невідома відеосистема: {videoSystem}", nameof(videoSystem));
        if (maxPowerMw <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxPowerMw), "Потужність має бути більшою за 0");
        if (frequencyGhz is not (1.2 or 2.4 or 3.3 or 5.8))
            throw new ArgumentOutOfRangeException(nameof(frequencyGhz), "Діапазон має бути 1.2, 2.4, 3.3 або 5.8 ГГц");
        if (!Mounts.Contains(mount))
            throw new ArgumentException($"Невідоме кріплення: {mount}", nameof(mount));

        VideoSystem = videoSystem;
        MaxPowerMw = maxPowerMw;
        FrequencyGhz = frequencyGhz;
        Mount = mount;
    }

    public string VideoSystem { get; }
    public int MaxPowerMw { get; }
    public double FrequencyGhz { get; }
    public string Mount { get; }

    public override ComponentType Type => ComponentType.Vtx;

    public override IReadOnlyDictionary<string, string> GetSpecs() => new Dictionary<string, string>
    {
        ["Відеосистема"] = VideoSystem,
        ["Макс. потужність"] = $"{MaxPowerMw} мВт",
        ["Діапазон"] = $"{FrequencyGhz} ГГц",
        ["Кріплення"] = Mount
    };
}
