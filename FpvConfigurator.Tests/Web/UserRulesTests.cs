using FpvConfigurator.Services.Rules.Custom;
using FpvConfigurator.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FpvConfigurator.Tests.Web;

/// <summary>Збереження правил користувача у файл rules.json (ФВ-28, TC-104).</summary>
public sealed class UserRulesTests : IDisposable
{
    private readonly WebTestData _data = new();

    public void Dispose() => _data.Dispose();

    private UserRules Start()
    {
        _data.StartApp(out var provider);
        return provider.GetRequiredService<UserRules>();
    }

    /// <summary>TC-104: правило на нестандартну категорію є і після «перезапуску», видалене правило зникає з файлу.</summary>
    [Fact]
    public void Rules_SurviveRestart()
    {
        var first = Start();
        // Receiver не є стандартною категорією, вона приходить з catalog/custom-parts.json.
        Assert.Empty(first.Add(new("Тільки 2.4", "Receiver", "frequencyGhz", RuleOperator.Equal, Constant: "2,4")));
        Assert.Empty(first.Add(new("ESC тягне мотори", "Esc", "continuousCurrentA", RuleOperator.GreaterOrEqual, "Motor", "maxCurrentA")));

        var second = Start();

        Assert.Empty(second.StartupErrors);
        Assert.Equal(["Тільки 2.4", "ESC тягне мотори"], second.Rules.Rules.Select(r => r.Name));

        Assert.True(second.Remove("Тільки 2.4"));
        Assert.Equal("ESC тягне мотори", Assert.Single(Start().Rules.Rules).Name);
    }

    /// <summary>TC-104: неправильне правило у файлі не зупиняє запуск, а потрапляє в StartupErrors.</summary>
    [Fact]
    public void BrokenRuleInFile_ReportedOnStart()
    {
        File.WriteAllText(_data.RulesFile, """
            [ { "name": "Підвіс", "leftCategory": "Gimbal", "leftParameter": "axes", "operator": "Greater", "constant": "1" },
              { "name": "Мінімум 4S", "leftCategory": "Battery", "leftParameter": "cells", "operator": "GreaterOrEqual", "constant": "4" } ]
            """);

        var rules = Start();

        Assert.Contains("Підвіс", Assert.Single(rules.StartupErrors));
        Assert.Equal("Мінімум 4S", Assert.Single(rules.Rules.Rules).Name);
    }
}