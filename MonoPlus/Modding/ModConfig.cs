using Chasm.SemanticVersioning;

namespace MonoPlus.Modding;

public class ModConfig
{
    public required string Name;
    public required string[] DllFiles;
    public required SemanticVersion Version;
}
