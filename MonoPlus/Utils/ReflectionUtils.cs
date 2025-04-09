using System;

namespace MonoPlus;

public static class ReflectionUtils
{
    public static T CreateInstance<T>() => Activator.CreateInstance<T>();

    public static T CreateInstance<T>(Type type) => (T)Activator.CreateInstance(type)!;
}
