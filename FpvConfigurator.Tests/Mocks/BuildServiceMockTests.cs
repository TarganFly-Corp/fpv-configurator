using FpvConfigurator.Catalog;
using FpvConfigurator.Core;
using FpvConfigurator.Services;
using FpvConfigurator.Tests.Mocks.Wrappers;
using FpvConfigurator.Tests.TestData;
using Moq;

namespace FpvConfigurator.Tests.Mocks;

/// <summary>BuildService з макетами каталогу і правил сумісності (TC-61…TC-65).</summary>
public class BuildServiceMockTests
{
    /// <summary>TC-61: деталь є в каталозі, Id у різному регістрі.</summary>
    [Theory]
    [InlineData("mt-1")]
    [InlineData("MT-1")]
    [InlineData("Mt-1")]
    public void AddPart_PartInCatalog_AddsToBuild(string partId)
    {
        var motor = TestParts.Motor();
        var catalog = new CatalogMock(TestParts.Frame(), motor);
        var service = new BuildService(catalog.Object, []);
        var build = new Build();

        var added = service.AddPart(build, partId);

        Assert.Same(motor, added);
        Assert.Same(motor, Assert.Single(build.Parts));
        catalog.Mock.Verify(c => c.FindById(partId), Times.Once());
    }

    /// <summary>TC-62: деталі немає в каталозі.</summary>
    [Theory]
    [InlineData("mt-404")]
    [InlineData("")]
    [InlineData("mt-1 ")]
    public void AddPart_PartNotInCatalog_ThrowsAndBuildUnchanged(string partId)
    {
        var service = new BuildService(new CatalogMock(TestParts.Motor()).Object, []);
        var build = new Build();

        var ex = Assert.Throws<KeyNotFoundException>(() => service.AddPart(build, partId));

        Assert.Contains($"«{partId}»", ex.Message);
        Assert.Empty(build.Parts);
    }

    /// <summary>TC-63: деталь є в каталозі, але її категорія у збірці вже заповнена.</summary>
    [Theory]
    [InlineData(ComponentType.Motor, "mt-1", 4)]
    [InlineData(ComponentType.Frame, "fr-1", 1)]
    [InlineData(ComponentType.Battery, "bt-1", 1)]
    public void AddPart_CategoryFull_ThrowsFromBuild(ComponentType type, string partId, int limit)
    {
        var build = new Build();
        for (var i = 0; i < limit; i++)
            build.Add(TestParts.OfType(type));
        var service = new BuildService(new CatalogMock(TestParts.OfType(type)).Object, []);

        Assert.Throws<InvalidOperationException>(() => service.AddPart(build, partId));

        Assert.Equal(limit, build.CountOf(type));
    }

    /// <summary>TC-64: звіт збирає проблеми всіх правил. Масив задає, скільки проблем повертає кожне правило.</summary>
    [Theory]
    [InlineData(new int[0], 0)]
    [InlineData(new[] { 0, 0 }, 0)]
    [InlineData(new[] { 1, 0 }, 1)]
    [InlineData(new[] { 2, 3 }, 5)]
    [InlineData(new[] { 0, 0, 0, 1 }, 1)]
    public void Check_ProblemsFromAllRules_Collected(int[] problemsPerRule, int expectedProblems)
    {
        var build = new Build();
        build.Add(TestParts.Frame());
        var rules = RuleMock.WithProblemCounts(problemsPerRule);
        // Каталог Strict: Check не має звертатися до каталогу взагалі
        var service = new BuildService(new Mock<IComponentCatalog>(MockBehavior.Strict).Object, rules.Select(r => r.Object));

        var report = service.Check(build);

        Assert.Equal(expectedProblems, report.Problems.Count);
        Assert.Equal(expectedProblems == 0, report.IsCompatible);
        foreach (var rule in rules)
            rule.Verify(r => r.Check(build.Parts), Times.Once());
    }

    /// <summary>TC-65: проблема підписана назвою правила, ціна і вага рахуються зі збірки.</summary>
    [Fact]
    public void Check_ProblemNamedByRule_TotalsFromBuild()
    {
        var build = new Build();
        build.Add(TestParts.Frame());
        build.Add(TestParts.Battery());
        var rule = RuleMock.Returning("Тестове правило", "батарея завелика");
        var service = new BuildService(new CatalogMock().Object, [rule.Object]);

        var report = service.Check(build);

        var problem = Assert.Single(report.Problems);
        Assert.Equal("Тестове правило", problem.Rule);
        Assert.Equal("батарея завелика", problem.Message);
        Assert.Equal(2700m, report.TotalPrice);
        Assert.Equal(320, report.TotalWeightGrams, precision: 3);
    }
}