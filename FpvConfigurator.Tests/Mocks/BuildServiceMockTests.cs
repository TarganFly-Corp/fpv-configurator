using FpvConfigurator.Catalog;
using FpvConfigurator.Core;
using FpvConfigurator.Services;
using FpvConfigurator.Tests.TestData;
using Moq;

namespace FpvConfigurator.Tests.Mocks;

/// <summary>BuildService з макетами каталогу і правил сумісності (TC-61…TC-65).</summary>
public class BuildServiceMockTests
{
    /// <summary>TC-61: деталь є в каталозі, Id у різному регістрі.</summary>
    [Theory(Skip = Stub.Reason)]
    [InlineData("mt-1")]
    [InlineData("MT-1")]
    public void AddPart_PartInCatalog_AddsToBuild(string partId)
    {
        var motor = TestParts.Motor();
        // TODO (розробник, #63): макет каталогу: на запит partId повертає мотор
        BuildService service = null!;
        var build = new Build();

        var added = service.AddPart(build, partId);

        Assert.Same(motor, added);
        Assert.Same(motor, Assert.Single(build.Parts));
    }

    /// <summary>TC-62: деталі немає в каталозі.</summary>
    [Theory(Skip = Stub.Reason)]
    [InlineData("mt-404")]
    [InlineData("")]
    public void AddPart_PartNotInCatalog_ThrowsAndBuildUnchanged(string partId)
    {
        // TODO (розробник, #63): макет каталогу: будь-який Id не знайдено
        BuildService service = null!;
        var build = new Build();

        var ex = Assert.Throws<KeyNotFoundException>(() => service.AddPart(build, partId));

        Assert.Contains($"«{partId}»", ex.Message);
        Assert.Empty(build.Parts);
    }

    /// <summary>TC-63: деталь є в каталозі, але її категорія у збірці вже заповнена.</summary>
    [Fact(Skip = Stub.Reason)]
    public void AddPart_CategoryFull_ThrowsFromBuild()
    {
        var build = new Build();
        for (var i = 0; i < 4; i++)
            build.Add(TestParts.Motor());
        // TODO (розробник, #63): макет каталогу
        BuildService service = null!;

        Assert.Throws<InvalidOperationException>(() => service.AddPart(build, "mt-1"));

        Assert.Equal(4, build.CountOf(ComponentType.Motor));
    }

    /// <summary>TC-64: звіт збирає проблеми всіх правил. Масив задає, скільки проблем повертає кожне правило.</summary>
    [Theory(Skip = Stub.Reason)]
    [InlineData(new int[0], 0)]
    [InlineData(new[] { 0, 0 }, 0)]
    [InlineData(new[] { 1, 0 }, 1)]
    [InlineData(new[] { 2, 3 }, 5)]
    [InlineData(new[] { 0, 0, 0, 1 }, 1)]
    public void Check_ProblemsFromAllRules_Collected(int[] problemsPerRule, int expectedProblems)
    {
        var build = new Build();
        build.Add(TestParts.Frame());
        // TODO (розробник, #63): макети правил: правило i повертає problemsPerRule[i] проблем.
        BuildService service = null!;

        var report = service.Check(build);

        Assert.Equal(expectedProblems, report.Problems.Count);
        Assert.Equal(expectedProblems == 0, report.IsCompatible);
    }

    /// <summary>TC-65: проблема підписана назвою правила, ціна і вага рахуються зі збірки.</summary>
    [Fact(Skip = Stub.Reason)]
    public void Check_ProblemNamedByRule_TotalsFromBuild()
    {
        var build = new Build();
        build.Add(TestParts.Frame());
        build.Add(TestParts.Battery());
        // TODO (розробник, #63): макет правила
        BuildService service = null!;

        var report = service.Check(build);

        var problem = Assert.Single(report.Problems);
        Assert.Equal("Тестове правило", problem.Rule);
        Assert.Equal("батарея завелика", problem.Message);
        Assert.Equal(2700m, report.TotalPrice);
        Assert.Equal(320, report.TotalWeightGrams, precision: 3);
    }
}