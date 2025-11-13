using System;
using System.Reflection;

namespace Monod.Utils.General;

/// <summary>
/// Helper class for simplier use of <see cref="System.Reflection"/>
/// </summary>
public static class ReflectionUtils
{
    /// <summary>
    /// Creates an instance of the type designated by the specified generic parameter
    /// </summary>
    /// <typeparam name="T">Type of class you want to create</typeparam>
    /// <returns>A reference to the newly created object</returns>
    public static T CreateInstance<T>() => Activator.CreateInstance<T>();

    /// <summary>
    /// Creates and instance of specified <see cref="Type"/>, and converts it into type specified with generic parameter
    /// </summary>
    /// <typeparam name="T">Type of class you want to get</typeparam>
    /// <param name="type">Type of class you want to create</param>
    /// <returns>A reference to the newly created object</returns>
    public static T CreateInstance<T>(Type type) => (T)Activator.CreateInstance(type)!;

    /// <summary>
    /// Finds all methods in specified <paramref name="assembly"/> with <paramref name="attributesTypes"/> attributes, and passes them to <paramref name="callback"/>
    /// </summary>
    /// <param name="assembly"><see cref="Assembly"/> to look in</param>
    /// <param name="attributesTypes"><see cref="Array"/> of <see cref="Type"/>s which inherit <see cref="Attribute"/></param>
    /// <param name="callback"><see cref="Action"/> which is called per attribute with type from <paramref name="attributesTypes"/> on method</param>
    /// <param name="publicOnly">Look only at types and methods which are <see langword="public"/></param>
    public static void FindMethodAttributes(Assembly assembly, Type[] attributesTypes, Action<MethodInfo, CustomAttributeData> callback, bool publicOnly = false)
    {
        Type[] types = publicOnly ? assembly.GetExportedTypes() : assembly.GetTypes();
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
        if (!publicOnly) flags |= BindingFlags.NonPublic;

        foreach (Type type in types)
            foreach (MethodInfo methodInfo in type.GetMethods(flags))
                foreach (CustomAttributeData attribute in methodInfo.CustomAttributes)
                    foreach (Type typeToFind in attributesTypes)
                        if (attribute.AttributeType.IsAssignableTo(typeToFind))
                            callback(methodInfo, attribute);
    }
}
