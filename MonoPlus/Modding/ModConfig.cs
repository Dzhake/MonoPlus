using Chasm.SemanticVersioning;

namespace MonoPlus.Modding;

public class ModConfig
{
    public required string Name;
    public required string AssemblyFile;
    public required SemanticVersion Version;
}
