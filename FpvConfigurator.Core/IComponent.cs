using System;
using System.Collections.Generic;
using System.Text;

namespace FpvConfigurator.Core
{
    /// <summary>Комплектуюча FPV-дрона. Реалізується класами в проєкті FpvConfigurator.Components.</summary>
    public interface IComponent
    {
        /// <summary>Унікальний ідентифікатор у каталозі.</summary>
        string Id { get; }

        /// <summary>Назва моделі.</summary>
        string Name { get; }

        /// <summary>Виробник.</summary>
        string Manufacturer { get; }

        /// <summary>Тип комплектуючої.</summary>
        ComponentType Type { get; }

        /// <summary>Ціна, грн.</summary>
        decimal Price { get; }

        /// <summary>Вага, г.</summary>
        double WeightGrams { get; }

        /// <summary>Чи є в наявності.</summary>
        bool InStock { get; }

        /// <summary>Технічні характеристики у вигляді «назва – значення».</summary>
        IReadOnlyDictionary<string, string> GetSpecs();
    }

}
