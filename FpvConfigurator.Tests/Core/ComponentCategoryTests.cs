using FpvConfigurator.Core;

namespace FpvConfigurator.Tests.Core;

public class ComponentCategoryTests
{
    private static ComponentCategory Gps() => new("Gps", "GPS-модуль",
    [
        new ParameterDefinition("chip", "Чип", ParameterType.Text),
        new ParameterDefinition("hasCompass", "Компас", ParameterType.Boolean)
    ]);

    [Fact]
    public void Validate_CorrectValues_NoErrors()
    {
        var errors = Gps().Validate(new Dictionary<string, object?> { ["chip"] = "M10", ["hasCompass"] = true });

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_MissingRequired_ReportsError()
    {
        var errors = Gps().Validate(new Dictionary<string, object?> { ["chip"] = "M10" });

        Assert.Single(errors);
    }

    [Fact]
    public void Validate_UnknownParameter_ReportsError()
    {
        var errors = Gps().Validate(new Dictionary<string, object?>
        {
            ["chip"] = "M10",
            ["hasCompass"] = false,
            ["color"] = "red"
        });

        Assert.Contains(errors, e => e.Contains("color"));
    }

    [Fact]
    public void Constructor_DuplicateParameterKeys_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ComponentCategory("X", "X",
        [
            new ParameterDefinition("a", "A", ParameterType.Text),
            new ParameterDefinition("a", "A2", ParameterType.Text)
        ]));
    }
}