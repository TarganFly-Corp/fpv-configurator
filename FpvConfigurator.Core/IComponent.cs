namespace FpvConfigurator.Core;

/// <summary>Комплектуюча FPV-дрона. Реалізується класами в проєкті FpvConfigurator.Components.</summary>
public interface IComponent
{
    /// <summary>Унікальний ідентифікатор у каталозі.</summary>
    string Id { get; }

    /// <summary>Назва моделі.</summary>
    string Name { get; }

    /// <summary>Виробник.</summary>
    string Manufacturer { get; }

    /// <summary>Тип комплектуючої. Для користувацьких деталей це ComponentType.Custom.</summary>
    ComponentType Type { get; }

    /// <summary>Ключ категорії в реєстрі. Для стандартних деталей збігається з назвою типу.</summary>
    string CategoryKey => Type.ToString();

    /// <summary>Ціна, грн.</summary>
    decimal Price { get; }

    /// <summary>Вага, г.</summary>
    double WeightGrams { get; }

    /// <summary>Чи є в наявності.</summary>
    bool InStock { get; }

    /// <summary>Значення параметрів за ключами зі схеми категорії.</summary>
    IReadOnlyDictionary<string, object?> Parameters => new Dictionary<string, object?>();

    /// <summary>Технічні характеристики у вигляді «назва: значення».</summary>
    IReadOnlyDictionary<string, string> GetSpecs();
}