using FpvConfigurator.Core;
using Moq;

namespace FpvConfigurator.Tests.Mocks.Wrappers;

/// <summary>Обгортка для макетів правил сумісності: правило з назвою, яке завжди повертає задані проблеми.</summary>
internal static class RuleMock
{
    public static Mock<ICompatibilityRule> Returning(string name, params string[] problems)
    {
        var rule = new Mock<ICompatibilityRule>();
        rule.Setup(r => r.Name).Returns(name);
        rule.Setup(r => r.Check(It.IsAny<IReadOnlyCollection<IComponent>>())).Returns(problems);
        return rule;
    }

    /// <summary>Правила «Правило 1», «Правило 2»…, i-те повертає counts[i] проблем.</summary>
    public static List<Mock<ICompatibilityRule>> WithProblemCounts(params int[] counts) =>
        counts.Select((count, i) => Returning($"Правило {i + 1}",
                Enumerable.Range(1, count).Select(k => $"проблема {k}").ToArray()))
              .ToList();
}