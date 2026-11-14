using FpvConfigurator.Core;

namespace FpvConfigurator.Tests.Core;

public class ParameterDefinitionTests
{
    [Fact]
    public void Validate_RequiredValueMissing_ReturnsError()
    {
        var p = new ParameterDefinition("kv", "KV", ParameterType.Integer);

        Assert.NotNull(p.Validate(null));
    }

    [Fact]
    public void Validate_OptionalValueMissing_ReturnsNull()
    {
        var p = new ParameterDefinition("telemetry", "Телеметрія", ParameterType.Boolean, required: false);

        Assert.Null(p.Validate(null));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(4, true)]
    [InlineData(5, false)]
    public void Validate_IntegerChecksRange(int value, bool isValid)
    {
        var p = new ParameterDefinition("antennas", "Антен", ParameterType.Integer, min: 1, max: 4);

        Assert.Equal(isValid, p.Validate(value) is null);
    }

    [Fact]
    public void Validate_IntegerRejectsFraction()
    {
        var p = new ParameterDefinition("antennas", "Антен", ParameterType.Integer);

        Assert.NotNull(p.Validate(2.5));
    }

    [Fact]
    public void Validate_NumberAcceptsIntAndDouble()
    {
        var p = new ParameterDefinition("freq", "Частота", ParameterType.Number, min: 0.8, max: 2.5);

        Assert.Null(p.Validate(2.4));
        Assert.Null(p.Validate(2));
    }

    [Fact]
    public void Validate_ChoiceAcceptsOnlyListedValues()
    {
        var p = new ParameterDefinition("protocol", "Протокол", ParameterType.Choice, options: ["ELRS", "Crossfire"]);

        Assert.Null(p.Validate("ELRS"));
        Assert.NotNull(p.Validate("PPM"));
    }

    [Fact]
    public void Constructor_ChoiceWithoutOptions_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ParameterDefinition("x", "X", ParameterType.Choice));
    }

    [Fact]
    public void Constructor_MinGreaterThanMax_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ParameterDefinition("x", "X", ParameterType.Number, min: 5, max: 1));
    }
}