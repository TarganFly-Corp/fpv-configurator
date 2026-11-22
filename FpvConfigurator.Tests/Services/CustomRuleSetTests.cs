using FpvConfigurator.Catalog;
using FpvConfigurator.Components;
using FpvConfigurator.Core;
using FpvConfigurator.Services;
using FpvConfigurator.Services.Rules.Custom;
using FpvConfigurator.Tests.TestData;
using Microsoft.Extensions.DependencyInjection;

namespace FpvConfigurator.Tests.Services;

/// <summary>Правила сумісності користувача (ФВ-28): перевірка опису і перевірка збірки (TC-100…TC-103).</summary>
public sealed class CustomRuleSetTests
{
    private readonly CustomRuleSet _rules = new(CategoryRegistry.WithStandardCategories());

    private static CustomRuleDefinition EscForMotors(string name = "ESC тягне мотори") =>
        new(name, "Esc", "continuousCurrentA", RuleOperator.GreaterOrEqual, "Motor", "maxCurrentA");

    private static Esc Esc(int current) =>
        new($"esc-{current}", $"ESC {current}A", "HGLRC", 1700m, 7, current, 2, 6, true, "20x20", "DShot600");

    private static Motor Motor(string id, double maxCurrent) =>
        new(id, $"Мотор {maxCurrent}A", "iFlight", 750m, 32, "2306", 2555, 3, 6, maxCurrent);

    /// <summary>TC-100: правильний опис приймається, дробове значення можна писати з комою.</summary>
    [Fact]
    public void Validate_CorrectRules_NoErrors()
    {
        Assert.Empty(_rules.Validate(EscForMotors()));
        Assert.Empty(_rules.Validate(new("Короткі пропи", "Propeller", "diameterInches", RuleOperator.LessOrEqual, Constant: "5,1")));
        Assert.Empty(_rules.Validate(new("Стек 30.5", "Frame", "stackMount", RuleOperator.Equal, Constant: "30.5x30.5")));
    }

    public static TheoryData<CustomRuleDefinition, string> InvalidRules => new()
    {
        { new("", "Esc", "continuousCurrentA", RuleOperator.Greater, Constant: "30"), "name" },
        { new("Без категорії", "Gimbal", "axes", RuleOperator.Greater, Constant: "1"), "left" },
        { new("Без параметра", "Esc", "current", RuleOperator.Greater, Constant: "30"), "left" },
        { new("Текст більше", "Frame", "stackMount", RuleOperator.Greater, "FlightController", "stackMount"), "operator" },
        { new("Число і текст", "Esc", "continuousCurrentA", RuleOperator.Equal, "Esc", "protocol"), "right" },
        { new("Не число", "Battery", "cells", RuleOperator.GreaterOrEqual, Constant: "abc"), "constant" },
        { new("Немає варіанта", "Frame", "stackMount", RuleOperator.Equal, Constant: "40x40"), "constant" },
        { new("Без значення", "Battery", "cells", RuleOperator.GreaterOrEqual), "constant" }
    };

    /// <summary>TC-101: неправильний опис дає помилку біля потрібного поля і не додається.</summary>
    [Theory]
    [MemberData(nameof(InvalidRules))]
    public void Validate_InvalidRule_ErrorForField(CustomRuleDefinition rule, string field)
    {
        var errors = _rules.Validate(rule);

        Assert.Contains(errors, e => e.Field == field);
        Assert.Throws<ArgumentException>(() => _rules.Add(rule));
        Assert.Empty(_rules.Rules);
    }

    /// <summary>TC-101: дві правила з однаковою назвою (регістр не важливий) не додаються.</summary>
    [Fact]
    public void Add_DuplicateName_Rejected()
    {
        _rules.Add(EscForMotors());

        var errors = _rules.Validate(EscForMotors("esc ТЯГНЕ мотори"));

        Assert.Equal("name", Assert.Single(errors).Field);
    }

    /// <summary>TC-102: ESC слабший за мотори дає одну проблему на пару деталей, навіть якщо моторів чотири.</summary>
    [Theory]
    [InlineData(35, 45, 1)]
    [InlineData(50, 40, 0)]
    [InlineData(40, 40, 0)]
    public void Check_EscAgainstMotors(int escCurrent, double motorCurrent, int expectedProblems)
    {
        _rules.Add(EscForMotors());
        var motor = Motor("mt-2", motorCurrent);
        IComponent[] build = [Esc(escCurrent), motor, motor, motor, motor, TestParts.Frame()];

        var problems = _rules.Check(build).ToList();

        Assert.Equal(expectedProblems, problems.Count);
        if (expectedProblems > 0)
        {
            Assert.StartsWith("«ESC тягне мотори»:", problems[0]);
            Assert.Contains($"ESC {escCurrent}A", problems[0]);
            Assert.Contains("≥", problems[0]);
        }
    }

    /// <summary>TC-102: правило не спрацьовує, якщо у збірці немає деталі однієї з категорій.</summary>
    [Fact]
    public void Check_MissingCategory_NoProblems()
    {
        _rules.Add(EscForMotors());

        Assert.Empty(_rules.Check([Esc(20), TestParts.Frame()]));
        Assert.Empty(_rules.Check([Motor("mt-1", 60)]));
    }

    /// <summary>TC-102: порівняння зі значенням: число і варіант зі списку (регістр не важливий).</summary>
    [Fact]
    public void Check_Constants()
    {
        _rules.Add(new("Мінімум 4S", "Battery", "cells", RuleOperator.GreaterOrEqual, Constant: "4"));
        _rules.Add(new("Стек 30.5", "Frame", "stackMount", RuleOperator.Equal, Constant: "30.5X30.5"));

        Assert.Empty(_rules.Check([TestParts.Battery(6), TestParts.Frame("30.5x30.5")]));
        var problems = _rules.Check([TestParts.Battery(3), TestParts.Frame("20x20")]).ToList();

        Assert.Equal(2, problems.Count);
        Assert.Contains(problems, p => p.StartsWith("«Мінімум 4S»") && p.Contains("≥ 4 S"));
        Assert.Contains(problems, p => p.StartsWith("«Стек 30.5»") && p.Contains("20x20"));
    }

    /// <summary>TC-102: видалене правило більше не перевіряє збірку.</summary>
    [Fact]
    public void Remove_RuleStopsChecking()
    {
        _rules.Add(EscForMotors());

        Assert.True(_rules.Remove("esc тягне мотори"));
        Assert.False(_rules.Remove("esc тягне мотори"));
        Assert.Empty(_rules.Check([Esc(20), Motor("mt-1", 60)]));
    }

    /// <summary>
    /// TC-103: контейнер передає BuildService той самий набір правил, що й редактору.
    /// Правило, додане після створення сервісу, одразу діє в його перевірці.
    /// </summary>
    [Fact]
    public void DependencyInjection_RuleAddedLater_UsedByBuildService()
    {
        var provider = new ServiceCollection().AddFpvConfigurator().BuildServiceProvider();
        var service = provider.GetRequiredService<BuildService>();
        var rules = provider.GetRequiredService<CustomRuleSet>();
        var build = new Build();
        build.Add(Esc(35));
        build.Add(Motor("mt-2", 45));

        Assert.Equal(5, service.Rules.Count);
        Assert.True(service.Check(build).IsCompatible);

        rules.Add(EscForMotors());
        var problem = Assert.Single(service.Check(build).Problems);

        Assert.Equal("Правила користувача", problem.Rule);
        Assert.Same(rules, provider.GetRequiredService<IEnumerable<ICompatibilityRule>>().OfType<CustomRuleSet>().Single());
    }
}