using FpvConfigurator.Catalog;
using FpvConfigurator.Components;

namespace FpvConfigurator.Tests.Components;

/// <summary>Межові значення конструктора Motor. Схема категорії: банок 1…12S, струм не менше 0 A.</summary>
public class MotorTests
{
    private static Motor Create(int minCells = 4, int maxCells = 6, double maxCurrentA = 40) =>
        new("mt", "2207 1950KV", "T-Motor", 900m, 33, "2207", 1950, minCells, maxCells, maxCurrentA);

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 12)]
    [InlineData(4, 6)]
    public void Constructor_CellRangeWithinSchema_Accepted(int minCells, int maxCells)
    {
        var motor = Create(minCells, maxCells);

        Assert.Empty(StandardCategories.Motor.Validate(motor.Parameters));
    }

    [Theory]
    [InlineData(0, 6)]
    [InlineData(6, 4)]
    [InlineData(4, 13)]
    public void Constructor_CellRangeOutsideSchema_Throws(int minCells, int maxCells)
    {
        Assert.ThrowsAny<ArgumentException>(() => Create(minCells, maxCells));
    }

    [Fact]
    public void Constructor_NegativeCurrent_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => Create(maxCurrentA: -5));
    }
}