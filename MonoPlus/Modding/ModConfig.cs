using Chasm.SemanticVersioning;

namespace MonoPlus.Modding;

public class ModConfig
{
    public required string Name;
    public required string AssemblyFile;
    public required SemanticVersion Version;
    public ModID?[]? HardDependencies;

    /// <summary>
    /// Call after mod was loaded successfully to clear dependencies from memory
    /// </summary>
    public void ClearDeps()
    {
        HardDependencies = null;
    }
}
