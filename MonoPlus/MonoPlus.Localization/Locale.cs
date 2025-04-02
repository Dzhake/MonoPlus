using System;
using System.Globalization;
using System.IO;
using Linguini.Bundle;
using Linguini.Bundle.Builder;
using Linguini.Bundle.Errors;

namespace MonoPlus.Localization;

public static class Locale
{
    private static FluentBundle? Bundle;

    public static void Initialize(string language)
    {
        Bundle = LinguiniBuilder.Builder().CultureInfo(CultureInfo.InvariantCulture).SkipResources().UncheckedBuild();
    }

    public static void Load(StreamReader stream)
    {
        if (Bundle is null) throw new InvalidOperationException("FluentBundle was not initialized, but Load was called!");
        Bundle.AddResource(stream, out var errors);

        if (errors is null) return;
        foreach (FluentError error in errors)
        {

        }
    }

    /*public static string Get(string key)
    {
        
    }*/


}
