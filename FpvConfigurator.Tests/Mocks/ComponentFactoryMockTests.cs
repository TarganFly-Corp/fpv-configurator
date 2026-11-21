using FpvConfigurator.Catalog;
using FpvConfigurator.Components;
using FpvConfigurator.Core;
using Moq;

namespace FpvConfigurator.Tests.Mocks;

/// <summary>ComponentFactory з макетом реєстру категорій (TC-70…TC-73).</summary>
public class ComponentFactoryMockTests
{
    private static readonly ComponentInfo Info = new("gb-1", "Mini Gimbal", "Maker", 800m, 25);

    /// <summary>Категорія, якої немає серед стандартних: підвіс з 1…3 осями.</summary>
    private static readonly ComponentCategory Gimbal = new("Gimbal", "Підвіс",
        [new ParameterDefinition("axes", "Осей", ParameterType.Integer, min: 1, max: 3)]);

    /// <summary>TC-70: значення на межах схеми з реєстру приймаються.</summary>
    [Theory(Skip = Stub.Reason)]
    [InlineData(1)]
    [InlineData(3)]
    public void Create_ValueWithinRegistrySchema_ReturnsCustomComponent(int axes)
    {
        // TODO (розробник, #63): макет реєстру: повертає категорію Gimbal
        ComponentFactory factory = null!;

        var part = factory.Create("Gimbal", Info, new Dictionary<string, object?> { ["axes"] = axes });

        var custom = Assert.IsType<CustomComponent>(part);
        Assert.Equal("Gimbal", custom.CategoryKey);
        Assert.Equal(axes, custom.Parameters["axes"]);
    }

    /// <summary>TC-71: значення за межами схеми відхиляються.</summary>
    [Theory(Skip = Stub.Reason)]
    [InlineData(0)]
    [InlineData(4)]
    public void Create_ValueOutsideRegistrySchema_Throws(int axes)
    {
        // TODO (розробник, #63): макет реєстру
        ComponentFactory factory = null!;

        var ex = Assert.Throws<ArgumentException>(() =>
            factory.Create("Gimbal", Info, new Dictionary<string, object?> { ["axes"] = axes }));

        Assert.Contains("Підвіс", ex.Message);
    }

    /// <summary>TC-72: реєстр не знає категорію, фабрика не ховає KeyNotFoundException.</summary>
    [Theory(Skip = Stub.Reason)]
    [InlineData("Gimbal")]
    [InlineData("frame")]
    public void Create_RegistryThrows_ExceptionPassedThrough(string categoryKey)
    {
        // TODO (розробник, #63): макет реєстру: будь-яка категорія невідома
        ComponentFactory factory = null!;

        Assert.Throws<KeyNotFoundException>(() =>
            factory.Create(categoryKey, Info, new Dictionary<string, object?>()));
    }

    /// <summary>TC-73: власний «конструктор» для категорії з реєстру викликається замість CustomComponent.</summary>
    [Fact(Skip = Stub.Reason)]
    public void RegisterCreator_CategoryFromRegistry_CreatorUsed()
    {
        // TODO (розробник, #63): макет реєстру
        ComponentFactory factory = null!;
        var expected = Mock.Of<IComponent>(c => c.Id == "gb-1");
        factory.RegisterCreator("Gimbal", (_, _) => expected);

        var part = factory.Create("Gimbal", Info, new Dictionary<string, object?> { ["axes"] = 2 });

        Assert.Same(expected, part);
    }
}