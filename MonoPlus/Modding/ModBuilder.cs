using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Serilog;

namespace MonoPlus.Modding;

/// <summary>
/// used to create <see cref="Mod"/>s
/// </summary>
public class ModBuilder(ModBuilder.ModCreationInfo info)
{
    /// <summary>
    /// Represents information about how the mod should be created with <see cref="CreateMod"/>
    /// </summary>
    public struct ModCreationInfo
    {
        /// <summary>
        /// Name of the mod
        /// </summary>
        public string Name;

        /// <summary>
        /// Whether the mod include code (.cs files)
        /// </summary>
        public bool IncludeCode;

        /// <summary>
        /// Dependencies of the mod
        /// </summary>
        public List<ModDep> Dependencies;
    }

    /// <summary>
    /// Information about how the mod should be created
    /// </summary>
    public ModCreationInfo Info = info;

    /// <summary>
    /// Creates a new mod (folder, files) based on specified <see cref="Info"/>
    /// </summary>
    /// <returns><see langword="null"/> on success, or <see cref="Exception"/> with information about fail otherwise</returns>
    public Exception? CreateMod()
    {
        Log.Information("Creating a new mod with name {ModName}", Info.Name);

        string modDir = Path.Combine(ModManager.ModsDirectory, Info.Name);
        if (Directory.Exists(modDir)) return new Exception($"Directory {modDir} already exists!");
        Directory.CreateDirectory(modDir);

        if (Info.IncludeCode)
        {
            Directory.CreateDirectory($"{modDir}Source/");
            WriteCsproj(File.CreateText($"{modDir}Source/{Info.Name}.csproj"));
        }

        ModConfig config = new()
        {
            AssemblyFile = Info.IncludeCode ? $"bin/{Info.Name}.dll" : null,
            Id = new(Info.Name, new(1, 0, 0)),
            Dependencies = Info.Dependencies,
        };

        FileStream configStream = new($"{modDir}config.json", FileMode.Create);
        JsonSerializer.Serialize(configStream, config);
        configStream.Close();

        return null;
    }

    private void WriteCsproj(StreamWriter writer)
    {
        writer.WriteLine("<Project Sdk=\"Microsoft.NET.Sdk\">");

        writer.WriteLine("  <PropertyGroup>");
        WriteProperties(writer);
        writer.WriteLine("  </PropertyGroup>");

        writer.WriteLine("  <ItemGroup>");
        WriteItems(writer);
        writer.WriteLine("  </ItemGroup>");

        WriteTasks(writer);

        writer.WriteLine("</Project>");
        writer.Close();
    }

    private void WriteProperties(StreamWriter writer)
    {
        writer.WriteLine($"    <AssemblyName>{Info.Name}</AssemblyName>");
        writer.WriteLine($"    <RootNamespace>{Info.Name}</RootNamespace>");
        writer.WriteLine("    <TargetFramework>net9.0</TargetFramework>");
        writer.WriteLine("    <LangVersion>preview</LangVersion>");
        writer.WriteLine("    <DebugType>embedded</DebugType>");
        writer.WriteLine("    <Nullable>enable</Nullable>");
        writer.WriteLine("    <AllowUnsafeBlocks>True</AllowUnsafeBlocks>");
    }

    private void WriteItems(StreamWriter writer)
    {
        writer.WriteLine("    <Reference Include=\"../../../MonoPlus.dll\">");
        writer.WriteLine("      <Private>false</Private>");
        writer.WriteLine("    </Reference>");
    }

    private void WriteTasks(StreamWriter writer)
    {
        writer.WriteLine("  <Target Name=\"CopyFiles\" AfterTargets=\"Build\">");
        writer.WriteLine("    <Copy SourceFiles=\"$(OutputPath)\\$(AssemblyName).dll\" DestinationFolder=\"bin\" />");
        writer.WriteLine("  </Target>");
    }
}
