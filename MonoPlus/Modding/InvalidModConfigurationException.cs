using System;

namespace MonoPlus.Modding;

public class InvalidModConfigurationException : Exception
{
    public string Issue;
    public string ConfigPath;

    /// <inheritdoc/>
    public override string Message => $"Invalid mod configuration at {ConfigPath}: {Issue}";

    public InvalidModConfigurationException(string configPath, string issue)
    {
        Issue = issue;
        ConfigPath = configPath;
    }
}
