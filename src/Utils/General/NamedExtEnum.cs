using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Monod.Utils.General;

public class NamedExtEnum
{
    protected readonly List<string> Names = new();

    public int AddValue(string name)
    {
        Names.Add(name);
        return Names.Count - 1;
    }

    public int? GetValue(string name)
    {
        for (int i = 0; i < Names.Count; i++)
            if (Names[i] == name)
                return i;
        return null;
    }

    public bool TryGetValue(string name, [NotNullWhen(true)] out int? value)
    {
        value = GetValue(name);
        return value != null;
    }

    public string GetName(int value) => Names[value];

    public bool TryGetName(int value, [NotNullWhen(true)]out string? name)
    {
        if (Names.Count >= value)
        {
            name = null;
            return false;
        }

        name = Names[value];
        return true;
    }
}