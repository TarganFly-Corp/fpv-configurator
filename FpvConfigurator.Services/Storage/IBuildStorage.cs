namespace FpvConfigurator.Services.Storage;

/// <summary>
/// Сховище збірок (ФВ-15). Реалізація з файлами JSON з'явиться у веб-застосунку (віха M6),
/// у модульних тестах сховище замінюється макетом Moq.
/// </summary>
public interface IBuildStorage
{
    /// <summary>Чи є збірка з такою назвою (регістр не важливий).</summary>
    bool Exists(string name);

    /// <summary>Записує збірку. Якщо збірка з такою назвою вже є, перезаписує її.</summary>
    void Save(SavedBuild build);

    /// <summary>Повертає збірку за назвою або null, якщо такої немає.</summary>
    SavedBuild? Load(string name);

    /// <summary>Назви всіх збережених збірок.</summary>
    IReadOnlyList<string> ListNames();
}