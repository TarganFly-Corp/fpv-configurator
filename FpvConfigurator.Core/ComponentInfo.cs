namespace FpvConfigurator.Core;

/// <summary>Спільні дані будь-якої комплектуючої, без параметрів категорії.</summary>
public sealed record ComponentInfo(string Id, string Name, string Manufacturer,
                                   decimal Price, double WeightGrams, bool InStock = true);