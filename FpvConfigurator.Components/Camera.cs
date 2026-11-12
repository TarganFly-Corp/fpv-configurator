using FpvConfigurator.Core;

namespace FpvConfigurator.Components;

public class Camera : ComponentBase
{
    private static readonly string[] VideoSystems = { "Analog", "DJI", "Walksnail", "HDZero" };
    private static readonly int[] Widths = { 14, 19 };

    public Camera(string id, string name, string manufacturer, decimal price, double weightGrams,
                  string videoSystem, int widthMm, string resolution, int fovDegrees)
        : base(id, name, manufacturer, price, weightGrams)
    {
        if (!VideoSystems.Contains(videoSystem))
            throw new ArgumentException($"Невідома відеосистема: {videoSystem}", nameof(videoSystem));
        if (!Widths.Contains(widthMm))
            throw new ArgumentOutOfRangeException(nameof(widthMm), "Ширина камери має бути 14 (nano) або 19 (micro) мм");
        if (string.IsNullOrWhiteSpace(resolution))
            throw new ArgumentException("Роздільність має бути вказана", nameof(resolution));
        if (fovDegrees is <= 0 or > 360)
            throw new ArgumentOutOfRangeException(nameof(fovDegrees), "Кут огляду має бути в межах 1–360°");

        VideoSystem = videoSystem;
        WidthMm = widthMm;
        Resolution = resolution;
        FovDegrees = fovDegrees;
    }

    public string VideoSystem { get; }
    public int WidthMm { get; }
    public string Resolution { get; }
    public int FovDegrees { get; }

    public override ComponentType Type => ComponentType.Camera;

    public override IReadOnlyDictionary<string, string> GetSpecs() => new Dictionary<string, string>
    {
        ["Відеосистема"] = VideoSystem,
        ["Ширина"] = $"{WidthMm} мм",
        ["Роздільність"] = Resolution,
        ["Кут огляду"] = $"{FovDegrees}°"
    };
}
