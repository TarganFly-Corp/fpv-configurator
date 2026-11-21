using FpvConfigurator.Core;
using FpvConfigurator.Services;
using FpvConfigurator.Tests.TestData;

namespace FpvConfigurator.Tests.Services;

/// <summary>Тести незалежного класу Build (ФВ-11, ФВ-14). ЛР6: ліміти категорій і порожня назва перевіряються наборами даних.</summary>
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

    /// <summary>TC-42, TC-43: деталь понад ліміт категорії. Ліміти: рама, контролер, ESC, камера, VTX, батарея 1; мотори і пропелери 4.</summary>
    public static TheoryData<ComponentType, int> CategoryLimits => new()
    {
        { ComponentType.Frame, 1 },
        { ComponentType.FlightController, 1 },
        { ComponentType.Esc, 1 },
        { ComponentType.Motor, 4 },
        { ComponentType.Propeller, 4 },
        { ComponentType.Camera, 1 },
        { ComponentType.Vtx, 1 },
        { ComponentType.Battery, 1 }
    };

    [Theory]
    [MemberData(nameof(CategoryLimits))]
    public void Add_OverCategoryLimit_ThrowsAndKeepsLimit(ComponentType type, int limit)
    {
        var build = new Build();
        for (var i = 0; i < limit; i++)
            build.Add(TestParts.OfType(type));

        var ex = Assert.Throws<InvalidOperationException>(() => build.Add(TestParts.OfType(type)));

        Assert.Contains(type.ToString(), ex.Message);
        Assert.Equal(limit, build.CountOf(type));
        Assert.Equal(limit, Build.LimitFor(type));
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

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Constructor_EmptyName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => new Build(name));
    }
}