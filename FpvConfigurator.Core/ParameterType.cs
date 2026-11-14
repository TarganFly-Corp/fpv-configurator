namespace FpvConfigurator.Core;

/// <summary>Тип значення параметра комплектуючої.</summary>
public enum ParameterType
{
    /// <summary>Довільний текст.</summary>
    Text,
    /// <summary>Ціле число.</summary>
    Integer,
    /// <summary>Дробове число.</summary>
    Number,
    /// <summary>Так або ні.</summary>
    Boolean,
    /// <summary>Одне значення зі списку Options.</summary>
    Choice
}