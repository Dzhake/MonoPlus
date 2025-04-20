using System;
using System.IO;

namespace MonoPlus.Modding;

/// <summary>
/// Thrown when config for <see cref="ModConfig"/> is not found or is invalid
/// </summary>
public class InvalidModConfigurationException : Exception
{
    /// <summary>
    /// Issue with the config
    /// </summary>
    public string Issue;

    /// <summary>
    /// <see cref="File"/> path where config is located
    /// </summary>
    public string ConfigPath;

    /// <inheritdoc/>
    public override string Message => $"Invalid mod configuration at {ConfigPath}: {Issue}";

    /// <summary>
    /// Instances a new <see cref="InvalidModConfigurationException"/> with specified issue and config path
    /// </summary>
    /// <param name="configPath"><see cref="File"/> path where config is located</param>
    /// <param name="issue">Issue with the config</param>
    public InvalidModConfigurationException(string configPath, string issue)
    {
        Issue = issue;
        ConfigPath = configPath;
    }
}
