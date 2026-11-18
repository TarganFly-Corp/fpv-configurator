using FpvConfigurator.Catalog;
using FpvConfigurator.Components;

namespace FpvConfigurator.Tests.Components;

/// <summary>
/// Межові значення конструкторів стандартних деталей (незалежні класи, тести без підміни).
/// Значення на межі схеми категорії приймаються, за межею відхиляються.
/// </summary>
public class ComponentBoundaryTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    public void Battery_CellsOnBoundary_Accepted(int cells)
    {
        var battery = new Battery("bt", "X", "Y", 100m, 200, cells, 1300, 100, "XT60");

        Assert.Empty(StandardCategories.Battery.Validate(battery.Parameters));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    public void Battery_CellsOutsideRange_Throws(int cells)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Battery("bt", "X", "Y", 100m, 200, cells, 1300, 100, "XT60"));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(6)]
    public void Propeller_BladesOnBoundary_Accepted(int blades)
    {
        var propeller = new Propeller("pr", "X", "Y", 100m, 4, 5.1, 4.6, blades);

        Assert.Empty(StandardCategories.Propeller.Validate(propeller.Parameters));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    public void Propeller_BladesOutsideRange_Throws(int blades)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Propeller("pr", "X", "Y", 100m, 4, 5.1, 4.6, blades));
    }

    [Theory]
    [InlineData(1, 12)]
    [InlineData(3, 6)]
    public void FlightController_CellRangeWithinSchema_Accepted(int minCells, int maxCells)
    {
        var fc = new FlightController("fc", "X", "Y", 100m, 8, "F722", "30.5x30.5", minCells, maxCells, "Betaflight", 5);

        Assert.Empty(StandardCategories.FlightController.Validate(fc.Parameters));
    }

    [Theory]
    [InlineData(0, 6)]
    [InlineData(6, 4)]
    [InlineData(3, 13)]
    public void FlightController_CellRangeOutsideSchema_Throws(int minCells, int maxCells)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new FlightController("fc", "X", "Y", 100m, 8, "F722", "30.5x30.5", minCells, maxCells, "Betaflight", 5));
    }

    [Theory]
    [InlineData(0, 6)]
    [InlineData(6, 4)]
    [InlineData(3, 13)]
    public void Esc_CellRangeOutsideSchema_Throws(int minCells, int maxCells)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new Esc("esc", "X", "Y", 100m, 12, 45, minCells, maxCells, true, "30.5x30.5", "DShot600"));
    }

    [Theory]
    [InlineData(14)]
    [InlineData(19)]
    public void Camera_StandardWidth_Accepted(int widthMm)
    {
        var camera = new Camera("cam", "X", "Y", 100m, 6, "Analog", widthMm, "1200TVL", 155);

        Assert.Empty(StandardCategories.Camera.Validate(camera.Parameters));
    }

    [Fact]
    public void Camera_NonStandardWidth_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => new Camera("cam", "X", "Y", 100m, 6, "Analog", 16, "1200TVL", 155));
    }

    [Theory]
    [InlineData(1.2)]
    [InlineData(5.8)]
    public void Vtx_StandardBand_Accepted(double frequencyGhz)
    {
        var vtx = new Vtx("vtx", "X", "Y", 100m, 5, "Analog", 800, frequencyGhz, "20x20");

        Assert.Empty(StandardCategories.Vtx.Validate(vtx.Parameters));
    }

    [Fact]
    public void Vtx_NonStandardBand_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => new Vtx("vtx", "X", "Y", 100m, 5, "Analog", 800, 5.0, "20x20"));
    }
}