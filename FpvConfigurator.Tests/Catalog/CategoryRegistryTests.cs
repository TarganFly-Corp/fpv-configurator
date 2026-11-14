using FpvConfigurator.Catalog;
using FpvConfigurator.Core;

namespace FpvConfigurator.Tests.Catalog;

public class CategoryRegistryTests
{
    [Fact]
    public void WithStandardCategories_ContainsEightCategories()
    {
        var registry = CategoryRegistry.WithStandardCategories();

        Assert.Equal(8, registry.All.Count);
        Assert.All(registry.All, c => Assert.True(c.IsStandard));
    }

    [Fact]
    public void Register_CustomCategory_CanBeFoundIgnoringCase()
    {
        var registry = CategoryRegistry.WithStandardCategories();
        registry.Register(new ComponentCategory("Gps", "GPS-модуль",
            [new ParameterDefinition("chip", "Чип", ParameterType.Text)]));

        Assert.True(registry.TryGet("gps", out var category));
        Assert.False(category.IsStandard);
    }

    [Fact]
    public void Register_DuplicateKey_Throws()
    {
        var registry = CategoryRegistry.WithStandardCategories();

        Assert.Throws<InvalidOperationException>(() =>
            registry.Register(new ComponentCategory("Frame", "Ще одна рама", [])));
    }

    [Fact]
    public void Get_UnknownKey_ThrowsKeyNotFound()
    {
        Assert.Throws<KeyNotFoundException>(() => new CategoryRegistry().Get("Gimbal"));
    }
}