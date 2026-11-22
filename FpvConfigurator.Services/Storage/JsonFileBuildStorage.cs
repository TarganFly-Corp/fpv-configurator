using System.Text.Encodings.Web;
using System.Text.Json;

namespace FpvConfigurator.Services.Storage;

/// <summary>
/// Сховище збірок в одному файлі JSON (ФВ-20). Файл перечитується під час створення об'єкта
/// і переписується після кожної зміни, тому збірки зберігаються між запусками програми.
/// Назви нечутливі до регістру.
/// </summary>
public sealed class JsonFileBuildStorage : IBuildStorage
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly string _path;
    private readonly Lock _lock = new();
    private readonly Dictionary<string, SavedBuild> _builds = new(StringComparer.OrdinalIgnoreCase);

    public JsonFileBuildStorage(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Шлях до файлу сховища не може бути порожнім", nameof(path));
        _path = Path.GetFullPath(path);

        if (!File.Exists(_path))
            return;
        var saved = JsonSerializer.Deserialize<List<SavedBuild>>(File.ReadAllText(_path), Options)
                    ?? throw new InvalidDataException($"Файл сховища {_path} порожній");
        foreach (var build in saved)
            _builds[build.Name] = build;
    }

    /// <summary>Повний шлях до файлу сховища.</summary>
    public string FilePath => _path;

    public bool Exists(string name)
    {
        lock (_lock)
            return _builds.ContainsKey(name);
    }

    public void Save(SavedBuild build)
    {
        ArgumentNullException.ThrowIfNull(build);
        if (string.IsNullOrWhiteSpace(build.Name))
            throw new ArgumentException("Назва збірки не може бути порожньою", nameof(build));
        lock (_lock)
        {
            _builds.Remove(build.Name); // щоб нова назва замінила стару, якщо різниця лише в регістрі
            _builds[build.Name] = new SavedBuild(build.Name, build.PartIds.ToList());
            Flush();
        }
    }

    public SavedBuild? Load(string name)
    {
        lock (_lock)
            return _builds.GetValueOrDefault(name);
    }

    public IReadOnlyList<string> ListNames()
    {
        lock (_lock)
            return _builds.Values.Select(b => b.Name).ToList();
    }

    public bool Delete(string name)
    {
        lock (_lock)
        {
            if (!_builds.Remove(name))
                return false;
            Flush();
            return true;
        }
    }

    private void Flush()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var temp = _path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(_builds.Values.ToList(), Options));
        File.Move(temp, _path, overwrite: true);
    }
}