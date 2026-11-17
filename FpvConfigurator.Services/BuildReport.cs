namespace FpvConfigurator.Services;

/// <summary>Одна проблема сумісності: яке правило її знайшло і що не так.</summary>
public sealed record BuildProblem(string Rule, string Message)
{
    public override string ToString() => $"{Rule}: {Message}";
}

/// <summary>Результат перевірки збірки (ФВ-12, ФВ-14).</summary>
public sealed record BuildReport(IReadOnlyList<BuildProblem> Problems, decimal TotalPrice, double TotalWeightGrams)
{
    public bool IsCompatible => Problems.Count == 0;
}