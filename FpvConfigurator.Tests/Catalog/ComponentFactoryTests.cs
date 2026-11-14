using FpvConfigurator.Catalog;
using FpvConfigurator.Components;
using FpvConfigurator.Core;

namespace FpvConfigurator.Tests.Catalog;

public class ComponentFactoryTests
{
    private static readonly ComponentInfo Info = new("id-1", "Test", "Maker", 100m, 10);

    private static (CategoryRegistry, ComponentFactory) Create()
    {
        var registry = CategoryRegistry.WithStandardCategories();
        return (registry, ComponentFactory.WithStandardCreators(registry));
    }

    [Fact]
    public void Create_StandardCategory_ReturnsTypedClass()
    {
        var (_, factory) = Create();

        var part = factory.Create("Frame", Info, new Dictionary<string, object?>
        {
            ["maxPropInches"] = 5.0,
            ["stackMount"] = "30.5x30.5",
            ["wheelbaseMm"] = 225
        });

        var frame = Assert.IsType<Frame>(part);
        Assert.Equal(225, frame.WheelbaseMm);
    }

    [Fact]
    public void Create_CustomCategory_ReturnsCustomComponent()
    {
        var (registry, factory) = Create();
        registry.Register(new ComponentCategory("Gps", "GPS-модуль",
            [new ParameterDefinition("hasCompass", "Компас", ParameterType.Boolean)]));

        var part = factory.Create("Gps", Info, new Dictionary<string, object?> { ["hasCompass"] = true });

        Assert.IsType<CustomComponent>(part);
        Assert.Equal(ComponentType.Custom, part.Type);
        Assert.Equal("Gps", part.CategoryKey);
        Assert.Equal("так", part.GetSpecs()["Компас"]);
    }

    [Fact]
    public void Create_InvalidParameters_Throws()
    {
        var (_, factory) = Create();

        Assert.Throws<ArgumentException>(() => factory.Create("Frame", Info, new Dictionary<string, object?>
        {
            ["maxPropInches"] = 5.0,
            ["stackMount"] = "16x16",
            ["wheelbaseMm"] = 225
        }));
    }

    [Fact]
    public void StandardClasses_ParametersMatchTheirSchema()
    {
        IComponent[] parts =
        [
            new Frame("f", "X", "Y", 1m, 1, 5, "30.5x30.5", 225),
            new Motor("m", "X", "Y", 1m, 1, "2207", 1950, 4, 6, 40),
            new Propeller("p", "X", "Y", 1m, 1, 5, 4.3, 3),
            new Battery("b", "X", "Y", 1m, 1, 6, 1300, 100, "XT60"),
            new FlightController("fc", "X", "Y", 1m, 1, "F722", "30.5x30.5", 3, 6, "Betaflight", 5),
            new Esc("e", "X", "Y", 1m, 1, 45, 3, 6, true, "30.5x30.5", "DShot600"),
            new Camera("c", "X", "Y", 1m, 1, "Analog", 19, "1200TVL", 155),
            new Vtx("v", "X", "Y", 1m, 1, "Analog", 800, 5.8, "20x20")
        ];
        var registry = CategoryRegistry.WithStandardCategories();

        foreach (var part in parts)
            Assert.Empty(registry.Get(part.CategoryKey).Validate(part.Parameters));
    }
}