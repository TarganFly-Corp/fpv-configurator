using FpvConfigurator.Core;

namespace FpvConfigurator.Catalog;

/// <summary>Схеми параметрів восьми стандартних категорій. Ключі збігаються з Parameters у класах Components.</summary>
public static class StandardCategories
{
    private static readonly string[] StackMounts = ["20x20", "25.5x25.5", "30.5x30.5"];
    private static readonly string[] VideoSystems = ["Analog", "DJI", "Walksnail", "HDZero"];

    public static readonly ComponentCategory Frame = new(nameof(ComponentType.Frame), "Рама",
    [
        new("maxPropInches", "Макс. гвинт", ParameterType.Number, "\"", min: 1),
        new("stackMount", "Кріплення стеку", ParameterType.Choice, options: StackMounts),
        new("wheelbaseMm", "Колісна база", ParameterType.Integer, "мм", min: 1)
    ], isStandard: true);

    public static readonly ComponentCategory Motor = new(nameof(ComponentType.Motor), "Мотор",
    [
        new("statorSize", "Статор", ParameterType.Text),
        new("kv", "KV", ParameterType.Integer, min: 1),
        new("minCells", "Мін. банок", ParameterType.Integer, "S", min: 1, max: 12),
        new("maxCells", "Макс. банок", ParameterType.Integer, "S", min: 1, max: 12),
        new("maxCurrentA", "Макс. струм", ParameterType.Number, "A", min: 0)
    ], isStandard: true);

    public static readonly ComponentCategory Propeller = new(nameof(ComponentType.Propeller), "Пропелер",
    [
        new("diameterInches", "Діаметр", ParameterType.Number, "\"", min: 0.1),
        new("pitch", "Крок", ParameterType.Number, min: 0),
        new("blades", "Лопатей", ParameterType.Integer, min: 2, max: 6)
    ], isStandard: true);

    public static readonly ComponentCategory Battery = new(nameof(ComponentType.Battery), "Батарея",
    [
        new("cells", "Банок", ParameterType.Integer, "S", min: 1, max: 8),
        new("capacityMah", "Ємність", ParameterType.Integer, "мАг", min: 1),
        new("cRating", "Струмовіддача", ParameterType.Integer, "C", min: 1),
        new("connector", "Роз'єм", ParameterType.Text)
    ], isStandard: true);

    public static readonly ComponentCategory FlightController = new(nameof(ComponentType.FlightController), "Польотний контролер",
    [
        new("mcu", "Процесор", ParameterType.Text),
        new("stackMount", "Кріплення стеку", ParameterType.Choice, options: StackMounts),
        new("minCells", "Мін. банок", ParameterType.Integer, "S", min: 1, max: 12),
        new("maxCells", "Макс. банок", ParameterType.Integer, "S", min: 1, max: 12),
        new("firmware", "Прошивка", ParameterType.Choice, options: ["Betaflight", "INAV", "ArduPilot"]),
        new("uartCount", "UART", ParameterType.Integer, min: 1)
    ], isStandard: true);

    public static readonly ComponentCategory Esc = new(nameof(ComponentType.Esc), "Регулятор обертів (ESC)",
    [
        new("continuousCurrentA", "Струм", ParameterType.Integer, "A", min: 1),
        new("minCells", "Мін. банок", ParameterType.Integer, "S", min: 1, max: 12),
        new("maxCells", "Макс. банок", ParameterType.Integer, "S", min: 1, max: 12),
        new("isFourInOne", "4-в-1", ParameterType.Boolean),
        new("stackMount", "Кріплення стеку", ParameterType.Choice, required: false, options: StackMounts),
        new("protocol", "Протокол", ParameterType.Choice, options: ["DShot150", "DShot300", "DShot600", "DShot1200"])
    ], isStandard: true);

    public static readonly ComponentCategory Camera = new(nameof(ComponentType.Camera), "Камера",
    [
        new("videoSystem", "Відеосистема", ParameterType.Choice, options: VideoSystems),
        new("widthMm", "Ширина", ParameterType.Integer, "мм", min: 14, max: 19),
        new("resolution", "Роздільність", ParameterType.Text),
        new("fovDegrees", "Кут огляду", ParameterType.Integer, "°", min: 1, max: 360)
    ], isStandard: true);

    public static readonly ComponentCategory Vtx = new(nameof(ComponentType.Vtx), "Відеопередавач (VTX)",
    [
        new("videoSystem", "Відеосистема", ParameterType.Choice, options: VideoSystems),
        new("maxPowerMw", "Макс. потужність", ParameterType.Integer, "мВт", min: 1),
        new("frequencyGhz", "Діапазон", ParameterType.Number, "ГГц", min: 1.2, max: 5.8),
        new("mount", "Кріплення", ParameterType.Choice, options: StackMounts)
    ], isStandard: true);

    public static IReadOnlyList<ComponentCategory> All { get; } =
        [Frame, FlightController, Esc, Motor, Propeller, Camera, Vtx, Battery];
}