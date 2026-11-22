using FpvConfigurator.Services.Rules.Custom;
using System.Text.Json;
using System.Text.Json.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;
using FpvConfigurator.Services.Rules.Custom;

namespace FpvConfigurator.Web.Services;

/// <summary>
/// Редактор правил сумісності користувача (ФВ-28). Додає і видаляє правила в CustomRuleSet,
/// який уже зареєстрований у BuildService, і зберігає їх у файл, щоб вони діяли після перезапуску.
/// </summary>
public sealed class UserRules
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly CustomRuleSet _rules;
    private readonly string _file;
    private readonly List<string> _startupErrors = [];
    private readonly Lock _lock = new();

    public UserRules(CustomRuleSet rules, string file)
    {
        _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        if (string.IsNullOrWhiteSpace(file))
            throw new ArgumentException("Шлях до файлу правил не може бути порожнім", nameof(file));
        _file = Path.GetFullPath(file);
    }

    public CustomRuleSet Rules => _rules;

    /// <summary>Файл, у якому зберігаються правила користувача.</summary>
    public string File => _file;

    /// <summary>Правила з файлу, які не вдалося завантажити (наприклад, їхню категорію видалено).</summary>
    public IReadOnlyList<string> StartupErrors => _startupErrors;

    /// <summary>
    /// Завантажує правила з файлу. Викликати після UserCatalog.Initialize,
    /// бо правила можуть посилатися на категорії користувача.
    /// </summary>
    public void Initialize()
    {
        lock (_lock)
        {
            if (!System.IO.File.Exists(_file))
                return;
            List<CustomRuleDefinition>? saved;
            try
            {
                saved = JsonSerializer.Deserialize<List<CustomRuleDefinition>>(System.IO.File.ReadAllText(_file), JsonOptions);
            }
            catch (JsonException ex)
            {
                _startupErrors.Add($"{Path.GetFileName(_file)}: {ex.Message}");
                return;
            }
            foreach (var rule in saved ?? [])
            {
                var errors = _rules.Validate(rule);
                if (errors.Count == 0)
                    _rules.Add(rule);
                else
                    _startupErrors.Add($"Правило «{rule.Name}»: {errors[0].Message}");
            }
        }
    }

    /// <summary>Перевіряє і додає правило. Повертає помилки; якщо їх немає, правило вже діє і збережене.</summary>
    public IReadOnlyList<RuleError> Add(CustomRuleDefinition rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        lock (_lock)
        {
            var errors = _rules.Validate(rule);
            if (errors.Count > 0)
                return errors;
            _rules.Add(rule);
            Persist();
            return [];
        }
    }

    /// <summary>Видаляє правило за назвою і зберігає файл. Повертає false, якщо такого правила немає.</summary>
    public bool Remove(string name)
    {
        lock (_lock)
        {
            if (!_rules.Remove(name))
                return false;
            Persist();
            return true;
        }
    }

    private void Persist()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_file)!);
        var temp = _file + ".tmp";
        System.IO.File.WriteAllText(temp, JsonSerializer.Serialize(_rules.Rules, JsonOptions));
        System.IO.File.Move(temp, _file, overwrite: true);
    }
}