using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using FpvConfigurator.Core;

namespace FpvConfigurator.Components
{
    public abstract class ComponentBase : FpvConfigurator.Core.IComponent
    {
        protected ComponentBase(string id, string name, string manufacturer,
                                decimal price, double weightGrams, bool inStock = true)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id не може бути порожнім", nameof(id));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Назва не може бути порожньою", nameof(name));
            if (price < 0)
                throw new ArgumentOutOfRangeException(nameof(price), "Ціна не може бути від'ємною");
            if (weightGrams <= 0)
                throw new ArgumentOutOfRangeException(nameof(weightGrams), "Вага має бути більшою за 0");

            Id = id;
            Name = name;
            Manufacturer = manufacturer;
            Price = price;
            WeightGrams = weightGrams;
            InStock = inStock;
        }

        public string Id { get; }
        public string Name { get; }
        public string Manufacturer { get; }
        public decimal Price { get; }
        public double WeightGrams { get; }
        public bool InStock { get; }

        public abstract ComponentType Type { get; }
        public abstract IReadOnlyDictionary<string, string> GetSpecs();

        public override string ToString() =>
            $"{Type}: {Manufacturer} {Name}, {Price} UAH, {WeightGrams} г";
    }
}
