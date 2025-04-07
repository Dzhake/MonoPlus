using System.IO;
using System.Text.Json.Nodes;

namespace MonoPlus.Modding;

public class ModConfig
{
    public string Name;

    /// <summary>
    /// Initializes a new <see cref="ModConfig"/> instance using provided <see cref="configStream"/>, and closes it.
    /// </summary>
    /// <param name="configStream">Stream which contains data in string-json format.</param>
    /// <param name="configPath">Config's file path, used for <see cref="InvalidModConfigurationException"/></param>
    public ModConfig(FileStream configStream, string configPath)
    {
        JsonNode? node = JsonNode.Parse(configStream.ToByteArrayDangerous());
        if (node is null) throw new InvalidModConfigurationException(configPath, "Config could not be parsed into JsonNode!");
        Name = node["name"]?.GetValue<string>();
        configStream.Close();
    }
}
