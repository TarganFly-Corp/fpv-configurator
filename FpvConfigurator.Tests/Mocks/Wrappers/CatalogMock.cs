using FpvConfigurator.Catalog;
using FpvConfigurator.Core;
using Moq;

namespace FpvConfigurator.Tests.Mocks.Wrappers;

/// <summary>
/// Обгортка над макетом IComponentCatalog: каталог, у якому є лише передані деталі.
/// FindById і Contains шукають Id без урахування регістру, як справжній ComponentCatalog.
/// </summary>
internal sealed class CatalogMock
{
    public CatalogMock(params IComponent[] parts)
    {
        Mock.Setup(c => c.FindById(It.IsAny<string>()))
            .Returns((string id) => parts.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase)));
        Mock.Setup(c => c.Contains(It.IsAny<string>()))
            .Returns((string id) => parts.Any(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase)));
    }

    public Mock<IComponentCatalog> Mock { get; } = new();

    public IComponentCatalog Object => Mock.Object;

    /// <summary>Перевіряє, скільки разів у каталог додавали деталі.</summary>
    public void VerifyAdded(Times times) => Mock.Verify(c => c.Add(It.IsAny<IComponent>()), times);

    /// <summary>Перевіряє, скільки разів шукали деталь за Id (будь-яким).</summary>
    public void VerifyLookups(Times times) => Mock.Verify(c => c.FindById(It.IsAny<string>()), times);
}