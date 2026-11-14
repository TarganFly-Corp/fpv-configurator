namespace FpvConfigurator.Core;

/// <summary>Тип комплектуючої FPV-дрона.</summary>
public enum ComponentType
{
    Frame,
    FlightController,
    Esc,
    Motor,
    Propeller,
    Camera,
    Vtx,
    Battery,

    /// <summary>Користувацька деталь. Її категорію описує ComponentCategory.</summary>
    Custom
}