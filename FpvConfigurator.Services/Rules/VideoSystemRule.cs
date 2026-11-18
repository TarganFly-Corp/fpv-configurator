using FpvConfigurator.Components;
using FpvConfigurator.Core;

namespace FpvConfigurator.Services.Rules;

/// <summary>Відеосистема камери збігається з відеосистемою передавача (ФВ-12).</summary>
public sealed class VideoSystemRule : ICompatibilityRule
{
    public string Name => "Відеосистема";

    public IEnumerable<string> Check(IReadOnlyCollection<IComponent> build)
    {
        ArgumentNullException.ThrowIfNull(build);
        var camera = build.OfType<Camera>().FirstOrDefault();
        var vtx = build.OfType<Vtx>().FirstOrDefault();

        if (camera is null || vtx is null || camera.VideoSystem == vtx.VideoSystem)
            return [];
        return [$"камера {camera.Name} ({camera.VideoSystem}) не працює з передавачем {vtx.Name} ({vtx.VideoSystem})"];
    }
}