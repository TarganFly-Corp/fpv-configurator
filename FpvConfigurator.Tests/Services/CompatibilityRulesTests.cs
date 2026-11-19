using FpvConfigurator.Core;
using FpvConfigurator.Services.Rules;
using FpvConfigurator.Tests.TestData;

namespace FpvConfigurator.Tests.Services;

/// <summary>Тести чотирьох правил сумісності (ФВ-12). Кожне правило перевіряється на справжніх деталях.</summary>
public class CompatibilityRulesTests
{
    [Fact]
    public void StackMount_AllMatch_NoProblems()
    {
        IComponent[] build = [TestParts.Frame(), TestParts.Fc(), TestParts.Esc()];

        Assert.Empty(new StackMountRule().Check(build));
    }

    [Fact]
    public void StackMount_Fc20x20OnFrame30_OneProblem()
    {
        IComponent[] build = [TestParts.Frame("30.5x30.5"), TestParts.Fc("20x20")];

        var problem = Assert.Single(new StackMountRule().Check(build));
        Assert.Contains("F722 HD", problem);
    }

    [Fact]
    public void StackMount_FourInOneEscDiffers_OneProblem()
    {
        IComponent[] build = [TestParts.Frame(), TestParts.Fc(), TestParts.Esc("20x20")];

        Assert.Single(new StackMountRule().Check(build));
    }

    [Fact]
    public void StackMount_SeparateEsc_Ignored()
    {
        IComponent[] build = [TestParts.Frame(), TestParts.Fc(), TestParts.Esc("20x20", fourInOne: false)];

        Assert.Empty(new StackMountRule().Check(build));
    }

    [Fact]
    public void PropellerSize_FourBigProps_OneProblem()
    {
        IComponent[] build = [TestParts.Frame(maxProp: 5.1), .. Enumerable.Range(0, 4).Select(_ => TestParts.Prop(7))];

        var problem = Assert.Single(new PropellerSizeRule().Check(build));
        Assert.Contains("7\"", problem);
    }

    [Fact]
    public void PropellerSize_PropFitsFrame_NoProblems()
    {
        IComponent[] build = [TestParts.Frame(maxProp: 5.1), TestParts.Prop(5.1)];

        Assert.Empty(new PropellerSizeRule().Check(build));
    }

    [Fact]
    public void CellCount_BatteryInAllRanges_NoProblems()
    {
        IComponent[] build = [TestParts.Battery(6), TestParts.Motor(4, 6), TestParts.Esc(minCells: 3, maxCells: 6), TestParts.Fc(minCells: 3, maxCells: 6)];

        Assert.Empty(new CellCountRule().Check(build));
    }

    [Fact]
    public void CellCount_BatteryAboveMotorAndEsc_TwoProblems()
    {
        IComponent[] build = [TestParts.Battery(6), TestParts.Motor(2, 4), TestParts.Esc(minCells: 2, maxCells: 4), TestParts.Fc()];

        var problems = new CellCountRule().Check(build).ToList();

        Assert.Equal(2, problems.Count);
        Assert.Contains(problems, p => p.Contains("2207 1950KV"));
        Assert.Contains(problems, p => p.Contains("BLS 50A"));
    }

    [Theory]
    [InlineData("Analog", "DJI", 1)]
    [InlineData("DJI", "DJI", 0)]
    public void VideoSystem_CameraAndVtx(string camera, string vtx, int expectedProblems)
    {
        IComponent[] build = [TestParts.Camera(camera), TestParts.Vtx(vtx)];

        Assert.Equal(expectedProblems, new VideoSystemRule().Check(build).Count());
    }

    [Fact]
    public void AllRules_FrameOnly_NoProblems()
    {
        IComponent[] build = [TestParts.Frame()];
        ICompatibilityRule[] rules = [new StackMountRule(), new PropellerSizeRule(), new CellCountRule(), new VideoSystemRule()];

        Assert.All(rules, rule => Assert.Empty(rule.Check(build)));
    }

    [Fact]
    public void AllRules_FullCompatibleSet_NoProblems()
    {
        var build = TestParts.FullSet();
        ICompatibilityRule[] rules = [new StackMountRule(), new PropellerSizeRule(), new CellCountRule(), new VideoSystemRule()];

        Assert.All(rules, rule => Assert.Empty(rule.Check(build)));
    }
}