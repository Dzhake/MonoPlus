using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MonoPlus.AssetsManagement.MSBuild;

public class CompileEffectsTask : Task
{
    public override bool Execute()
    {
        Log.LogMessage(MessageImportance.High, "Hello world!");
        File.Create("C:/idk.txt");
        return true;
    }
}