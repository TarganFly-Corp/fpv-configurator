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
    [Theory]
    [InlineData("mt-1")]
    [InlineData("MT-1")]
    public void AddPart_PartInCatalog_AddsToBuild(string partId)
    {
        var motor = TestParts.Motor();
        // Макет каталогу (розробник): на запит partId повертає мотор
        var catalog = new Mock<IComponentCatalog>();
        catalog.Setup(c => c.FindById(partId)).Returns(motor);
        var service = new BuildService(catalog.Object, []);
        var build = new Build();

        var added = service.AddPart(build, partId);

        Assert.Same(motor, added);
        Assert.Same(motor, Assert.Single(build.Parts));
        catalog.Verify(c => c.FindById(partId), Times.Once());
    }

    /// <summary>TC-62: деталі немає в каталозі.</summary>
    [Theory]
    [InlineData("mt-404")]
    [InlineData("")]
    public void AddPart_PartNotInCatalog_ThrowsAndBuildUnchanged(string partId)
    {
        // Макет каталогу (розробник): будь-який Id не знайдено
        var catalog = new Mock<IComponentCatalog>();
        catalog.Setup(c => c.FindById(It.IsAny<string>())).Returns((IComponent?)null);
        var service = new BuildService(catalog.Object, []);
        var build = new Build();

        var ex = Assert.Throws<KeyNotFoundException>(() => service.AddPart(build, partId));

        Assert.Contains($"«{partId}»", ex.Message);
        Assert.Empty(build.Parts);
    }

    /// <summary>TC-63: деталь є в каталозі, але її категорія у збірці вже заповнена.</summary>
    [Fact]
    public void AddPart_CategoryFull_ThrowsFromBuild()
    {
        var build = new Build();
        for (var i = 0; i < 4; i++)
            build.Add(TestParts.Motor());
        // Макет каталогу (розробник)
        var catalog = new Mock<IComponentCatalog>();
        catalog.Setup(c => c.FindById("mt-1")).Returns(TestParts.Motor());
        var service = new BuildService(catalog.Object, []);

        Assert.Throws<InvalidOperationException>(() => service.AddPart(build, "mt-1"));

        Assert.Equal(4, build.CountOf(ComponentType.Motor));
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
        // Макети правил (розробник): правило i повертає problemsPerRule[i] проблем.
        // Каталог Strict: Check не має звертатися до каталогу взагалі.
        var rules = problemsPerRule.Select((count, i) =>
        {
            var rule = new Mock<ICompatibilityRule>();
            rule.Setup(r => r.Name).Returns($"Правило {i + 1}");
            rule.Setup(r => r.Check(It.IsAny<IReadOnlyCollection<IComponent>>()))
                .Returns(Enumerable.Range(1, count).Select(k => $"проблема {k}").ToList());
            return rule;
        }).ToList();
        var catalog = new Mock<IComponentCatalog>(MockBehavior.Strict);
        var service = new BuildService(catalog.Object, rules.Select(r => r.Object));

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
        // Макет правила (розробник)
        var rule = new Mock<ICompatibilityRule>();
        rule.Setup(r => r.Name).Returns("Тестове правило");
        rule.Setup(r => r.Check(It.IsAny<IReadOnlyCollection<IComponent>>())).Returns(["батарея завелика"]);
        var service = new BuildService(Mock.Of<IComponentCatalog>(), [rule.Object]);

        var report = service.Check(build);

        var problem = Assert.Single(report.Problems);
        Assert.Equal("Тестове правило", problem.Rule);
        Assert.Equal("батарея завелика", problem.Message);
        Assert.Equal(2700m, report.TotalPrice);
        Assert.Equal(320, report.TotalWeightGrams, precision: 3);
    }
}