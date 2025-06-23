using System;
using System.IO;

namespace MonoPlus.ModSystem;

/// <summary>
/// Thrown when config for <see cref="ModConfig"/> is not found or is invalid
/// </summary>
public class InvalidModConfigException : Exception
{
    /// <summary>
    /// Issue with the config
    /// </summary>
    public readonly string Issue;

    /// <summary>
    /// <see cref="File"/> path where config is located
    /// </summary>
    public readonly string ConfigPath;

    /// <inheritdoc/>
    public override string Message => $"Invalid mod configuration at {ConfigPath}: {Issue}";

    /// <summary>
    /// Instances a new <see cref="InvalidModConfigException"/> with specified issue and config path
    /// </summary>
    /// <param name="configPath"><see cref="File"/> path where config is located</param>
    /// <param name="issue">Issue with the config</param>
    public InvalidModConfigException(string configPath, string issue)
    {
        Issue = issue;
        ConfigPath = configPath;
    }
}
