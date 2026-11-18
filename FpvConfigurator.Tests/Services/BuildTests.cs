using FpvConfigurator.Core;
using FpvConfigurator.Services;

namespace FpvConfigurator.Tests.Services;

/// <summary>Тести незалежного класу Build (ФВ-11, ФВ-14).</summary>
public class BuildTests
{
    [Fact]
    public void Add_FullStandardSet_AllPartsAccepted()
    {
        var build = new Build();

        foreach (var part in TestParts.FullSet())
            build.Add(part);

        Assert.Equal(14, build.Parts.Count);
        Assert.Equal(4, build.CountOf(ComponentType.Motor));
    }

    [Fact]
    public void Add_FifthMotor_ThrowsAndKeepsFour()
    {
        var build = new Build();
        for (var i = 0; i < 4; i++)
            build.Add(TestParts.Motor());

        var ex = Assert.Throws<InvalidOperationException>(() => build.Add(TestParts.Motor()));

        Assert.Contains("Motor", ex.Message);
        Assert.Equal(4, build.CountOf(ComponentType.Motor));
    }

    [Fact]
    public void Add_SecondFrame_Throws()
    {
        var build = new Build();
        build.Add(TestParts.Frame());

        Assert.Throws<InvalidOperationException>(() => build.Add(TestParts.Frame("20x20")));
    }

    [Fact]
    public void Add_CustomParts_NoLimit()
    {
        var build = new Build();

        for (var i = 1; i <= 5; i++)
            build.Add(TestParts.Gps($"gps-{i}"));

        Assert.Equal(5, build.CountOf(ComponentType.Custom));
        Assert.Null(Build.LimitFor(ComponentType.Custom));
    }

    [Fact]
    public void Remove_ByIdInOtherCase_RemovesOnePart()
    {
        var build = new Build();
        for (var i = 0; i < 4; i++)
            build.Add(TestParts.Motor());

        Assert.True(build.Remove("MT-1"));
        Assert.Equal(3, build.CountOf(ComponentType.Motor));
        Assert.False(build.Remove("mt-404"));
    }

    [Fact]
    public void Totals_SumPriceAndWeight()
    {
        var build = new Build();
        build.Add(TestParts.Frame());
        build.Add(TestParts.Battery());

        Assert.Equal(2700m, build.TotalPrice);
        Assert.Equal(320, build.TotalWeightGrams, precision: 3);
    }

    [Fact]
    public void Totals_EmptyBuild_Zero()
    {
        var build = new Build();

        Assert.Equal(0m, build.TotalPrice);
        Assert.Equal(0, build.TotalWeightGrams);
    }

    [Fact]
    public void Constructor_EmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Build(" "));
    }
}