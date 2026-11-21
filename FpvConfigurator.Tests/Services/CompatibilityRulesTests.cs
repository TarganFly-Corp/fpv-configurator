using FpvConfigurator.Core;
using FpvConfigurator.Services.Rules;
using FpvConfigurator.Tests.TestData;

namespace FpvConfigurator.Tests.Services;

/// <summary>
/// Тести чотирьох правил сумісності (ФВ-12). Кожне правило перевіряється на справжніх деталях.
/// ЛР6: окремі тести з однаковою схемою об'єднано в набори вхідних даних.
/// </summary>
public class CompatibilityRulesTests
{
    /// <summary>TC-47…TC-49: кріплення рами, контролера і ESC.</summary>
    [Theory]
    [InlineData("30.5x30.5", "30.5x30.5", "30.5x30.5", true, 0)]   // усе збігається
    [InlineData("30.5x30.5", "20x20", "20x20", true, 1)]           // контролер 20x20 на рамі 30.5
    [InlineData("30.5x30.5", "30.5x30.5", "20x20", true, 1)]       // ESC 4-в-1 іншого розміру
    [InlineData("30.5x30.5", "30.5x30.5", "20x20", false, 0)]      // окремий ESC не перевіряється
    [InlineData("20x20", "30.5x30.5", "20x20", true, 2)]           // контролер не стає ні на раму, ні на ESC
    public void StackMount_Check_ReturnsExpectedProblemCount(
        string frameMount, string fcMount, string escMount, bool fourInOne, int expectedProblems)
    {
        IComponent[] build = [TestParts.Frame(frameMount), TestParts.Fc(fcMount), TestParts.Esc(escMount, fourInOne)];

        Assert.Equal(expectedProblems, new StackMountRule().Check(build).Count());
    }

    /// <summary>TC-50: діаметр пропелера і максимальний пропелер рами 5.1".</summary>
    [Theory]
    [InlineData(5.1, 0)]   // на межі
    [InlineData(3.0, 0)]
    [InlineData(5.2, 1)]   // трохи більший за межу
    [InlineData(7.0, 1)]
    public void PropellerSize_FourPropsOnFrame51_ReturnsExpectedProblemCount(double diameter, int expectedProblems)
    {
        IComponent[] build = [TestParts.Frame(maxProp: 5.1), .. Enumerable.Range(0, 4).Select(_ => TestParts.Prop(diameter))];

        Assert.Equal(expectedProblems, new PropellerSizeRule().Check(build).Count());
    }

    /// <summary>TC-51, TC-52: банки батареї проти діапазонів мотора, ESC і контролера 3…6S або 4…6S.</summary>
    [Theory]
    [InlineData(6, 4, 6, 3, 6, 0)]   // усе в діапазоні
    [InlineData(4, 4, 6, 3, 6, 0)]   // нижня межа мотора
    [InlineData(3, 4, 6, 3, 6, 1)]   // нижче за мотор
    [InlineData(6, 2, 4, 2, 4, 2)]   // вище за мотор і ESC
    public void CellCount_Battery_ReturnsExpectedProblemCount(
        int cells, int motorMin, int motorMax, int escMin, int escMax, int expectedProblems)
    {
        IComponent[] build =
        [
            TestParts.Battery(cells), TestParts.Motor(motorMin, motorMax),
            TestParts.Esc(minCells: escMin, maxCells: escMax), TestParts.Fc(minCells: 3, maxCells: 6)
        ];

        Assert.Equal(expectedProblems, new CellCountRule().Check(build).Count());
    }

    [Fact]
    public void CellCount_BatteryAboveMotorAndEsc_ProblemsNamePartsInBuild()
    {
        IComponent[] build = [TestParts.Battery(6), TestParts.Motor(2, 4), TestParts.Esc(minCells: 2, maxCells: 4), TestParts.Fc()];

        var problems = new CellCountRule().Check(build).ToList();

        Assert.Contains(problems, p => p.Contains("2207 1950KV"));
        Assert.Contains(problems, p => p.Contains("BLS 50A"));
    }

    /// <summary>TC-53: відеосистема камери і VTX.</summary>
    [Theory]
    [InlineData("Analog", "DJI", 1)]
    [InlineData("DJI", "DJI", 0)]
    [InlineData("Analog", "Analog", 0)]
    [InlineData("HDZero", "Walksnail", 1)]
    public void VideoSystem_CameraAndVtx(string camera, string vtx, int expectedProblems)
    {
        IComponent[] build = [TestParts.Camera(camera), TestParts.Vtx(vtx)];

        Assert.Equal(expectedProblems, new VideoSystemRule().Check(build).Count());
    }

    /// <summary>TC-54: неповна і повна сумісна збірка.</summary>
    public static TheoryData<string, IComponent[]> CompatibleBuilds => new()
    {
        { "лише рама", [TestParts.Frame()] },
        { "повний набір", TestParts.FullSet() }
    };

    [Theory]
    [MemberData(nameof(CompatibleBuilds))]
    public void AllRules_CompatibleBuild_NoProblems(string description, IComponent[] build)
    {
        ICompatibilityRule[] rules = [new StackMountRule(), new PropellerSizeRule(), new CellCountRule(), new VideoSystemRule()];

        foreach (var rule in rules)
            Assert.False(rule.Check(build).Any(), $"{rule.Name}: є проблеми для збірки «{description}»");
    }
}