using FpvConfigurator.Components;
using FpvConfigurator.Core;

namespace FpvConfigurator.Tests.Services;

/// <summary>Справжні деталі для тестів збірки і правил. Підміни класів немає.</summary>
internal static class TestParts
{
    public static Frame Frame(string mount = "30.5x30.5", double maxProp = 5.1) =>
        new("fr-1", "Source One V5", "TBS", 1200m, 110, maxProp, mount, 225);

    public static FlightController Fc(string mount = "30.5x30.5", int minCells = 3, int maxCells = 6) =>
        new("fc-1", "F722 HD", "SpeedyBee", 1900m, 9, "STM32F722", mount, minCells, maxCells, "Betaflight", 5);

    public static Esc Esc(string mount = "30.5x30.5", bool fourInOne = true, int minCells = 3, int maxCells = 6) =>
        new("esc-1", "BLS 50A", "SpeedyBee", 2200m, 14, 50, minCells, maxCells, fourInOne, mount, "DShot600");

    public static Motor Motor(int minCells = 4, int maxCells = 6) =>
        new("mt-1", "2207 1950KV", "T-Motor", 900m, 33, "2207", 1950, minCells, maxCells, 40);

    public static Propeller Prop(double diameter = 5.1) =>
        new("pr-1", "51466", "Gemfan", 120m, 4.2, diameter, 4.66, 3);

    public static Camera Camera(string videoSystem = "Analog") =>
        new("cam-1", "Ratel 2", "Caddx", 1100m, 5.9, videoSystem, 19, "1200TVL", 165);

    public static Vtx Vtx(string videoSystem = "Analog") =>
        new("vtx-1", "Unify Pro32", "TBS", 1700m, 7, videoSystem, 1000, 5.8, "20x20");

    public static Battery Battery(int cells = 6) =>
        new("bt-1", "1300mAh 6S", "CNHL", 1500m, 210, cells, 1300, 100, "XT60");

    public static CustomComponent Gps(string id = "gps-1")
    {
        var category = new ComponentCategory("Gps", "GPS-модуль", [new ParameterDefinition("chip", "Чип", ParameterType.Text)]);
        return new CustomComponent(new ComponentInfo(id, "M10 Mini", "HGLRC", 900m, 5.8), category,
                                   new Dictionary<string, object?> { ["chip"] = "u-blox M10" });
    }

    /// <summary>Повний сумісний набір: 14 деталей.</summary>
    public static IComponent[] FullSet() =>
    [
        Frame(), Fc(), Esc(),
        Motor(), Motor(), Motor(), Motor(),
        Prop(), Prop(), Prop(), Prop(),
        Camera(), Vtx(), Battery()
    ];
}