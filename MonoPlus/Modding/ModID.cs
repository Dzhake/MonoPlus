using Chasm.SemanticVersioning;

namespace MonoPlus.Modding;

public struct ModID
{
    public string Name;
    public SemanticVersion Version;

    public bool Matches(ModID other)
    {
        return Name == other.Name && Version >= other.Version;
    }
}
