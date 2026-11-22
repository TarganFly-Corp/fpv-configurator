namespace FpvConfigurator.Services.Rules.Custom;

/// <summary>Операція порівняння у правилі користувача (ФВ-28).</summary>
public enum RuleOperator
{
    Equal,
    NotEqual,
    Less,
    LessOrEqual,
    Greater,
    GreaterOrEqual
}

public static class RuleOperatorExtensions
{
    /// <summary>Знак операції для інтерфейсу і повідомлень.</summary>
    public static string Symbol(this RuleOperator op) => op switch
    {
        RuleOperator.Equal => "=",
        RuleOperator.NotEqual => "≠",
        RuleOperator.Less => "<",
        RuleOperator.LessOrEqual => "≤",
        RuleOperator.Greater => ">",
        RuleOperator.GreaterOrEqual => "≥",
        _ => op.ToString()
    };

    /// <summary>Чи порівнює операція «більше/менше». Такі операції працюють лише з числами.</summary>
    public static bool IsOrdering(this RuleOperator op) => op is not (RuleOperator.Equal or RuleOperator.NotEqual);
}