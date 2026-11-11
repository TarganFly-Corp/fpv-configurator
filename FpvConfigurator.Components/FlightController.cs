using FpvConfigurator.Core;

namespace FpvConfigurator.Components;

public class FlightController : ComponentBase
{
    private static readonly string[] StackMounts = { "20x20", "25.5x25.5", "30.5x30.5" };
    private static readonly string[] Firmwares = { "Betaflight", "INAV", "ArduPilot" };

    public FlightController(string id, string name, string manufacturer, decimal price, double weightGrams,
                            string mcu, string stackMount, int minCells, int maxCells,
                            string firmware, int uartCount)
        : base(id, name, manufacturer, price, weightGrams)
    {
        if (string.IsNullOrWhiteSpace(mcu))
            throw new ArgumentException("Процесор (MCU) має бути вказаний", nameof(mcu));
        if (!StackMounts.Contains(stackMount))
            throw new ArgumentException($"Невідоме кріплення стеку: {stackMount}", nameof(stackMount));
        if (minCells < 1 || maxCells > 12 || minCells > maxCells)
            throw new ArgumentOutOfRangeException(nameof(minCells), "Діапазон банок має бути в межах 1–12S і min ≤ max");
        if (!Firmwares.Contains(firmware))
            throw new ArgumentException($"Невідома прошивка: {firmware}", nameof(firmware));
        if (uartCount < 1)
            throw new ArgumentOutOfRangeException(nameof(uartCount), "Має бути хоча б один UART");

        Mcu = mcu;
        StackMount = stackMount;
        MinCells = minCells;
        MaxCells = maxCells;
        Firmware = firmware;
        UartCount = uartCount;
    }

    public string Mcu { get; }
    public string StackMount { get; }
    public int MinCells { get; }
    public int MaxCells { get; }
    public string Firmware { get; }
    public int UartCount { get; }

    public override ComponentType Type => ComponentType.FlightController;

    public override IReadOnlyDictionary<string, string> GetSpecs() => new Dictionary<string, string>
    {
        ["Процесор"] = Mcu,
        ["Кріплення стеку"] = StackMount,
        ["Живлення"] = $"{MinCells}–{MaxCells}S",
        ["Прошивка"] = Firmware,
        ["UART"] = UartCount.ToString()
    };
}
