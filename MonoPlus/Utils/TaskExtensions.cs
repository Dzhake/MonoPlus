/*﻿ MIT License
   
   Copyright (c) 2022-2024 Chasmical
   
   Permission is hereby granted, free of charge, to any person obtaining a copy
   of this software and associated documentation files (the "Software"), to deal
   in the Software without restriction, including without limitation the rights
   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
   copies of the Software, and to permit persons to whom the Software is
   furnished to do so, subject to the following conditions:
   
   The above copyright notice and this permission notice shall be included in all
   copies or substantial portions of the Software.
*/
﻿using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace MonoPlus;

/// <summary>
///   <para>Provides a set of extension methods for <see cref="Task{T}"/> and related types.</para>
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    ///   <para>Transitions the <paramref name="source"/>'s underlying task to a completion state corresponding to the result of the specified <paramref name="func"/>.</para>
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the task.</typeparam>
    /// <param name="source">The task completion source to transition to a completion state.</param>
    /// <param name="func">The function whose result should be used as a completion state for the <paramref name="source"/>'s underlying task.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="func"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The <paramref name="source"/>'s underlying task has already completed.</exception>
    public static void SetFromFunc<T>(this TaskCompletionSource<T> source, [InstantHandle] Func<T> func)
    {
        const string msg = "An attempt was made to transition a task to a final state when it had already completed.";
        if (!TrySetFromFunc(source, func)) throw new InvalidOperationException(msg);
    }
    /// <summary>
    ///   <para>Attempts to transition the <paramref name="source"/>'s underlying task to a completion state corresponding to the result of the specified <paramref name="func"/>.</para>
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the task.</typeparam>
    /// <param name="source">The task completion source to transition to a completion state.</param>
    /// <param name="func">The function whose result should be used as a completion state for the <paramref name="source"/>'s underlying task.</param>
    /// <returns><see langword="true"/>, if the operation was successful; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="func"/> is <see langword="null"/>.</exception>
    [MustUseReturnValue]
    public static bool TrySetFromFunc<T>(this TaskCompletionSource<T> source, [InstantHandle] Func<T> func)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(func);

        if (source.Task.IsCompleted) return false;

        T result;
        try
        {
            result = func();
        }
        catch (Exception ex)
        {
            return source.TrySetException(ex);
        }
        return source.TrySetResult(result);
    }

    /// <summary>
    /// Cast <paramref name="task"/> to <see cref="Task{T}"/> via <see cref="Unsafe"/>.
    /// </summary>
    /// <typeparam name="T">Return type of new task.</typeparam>
    /// <param name="task">Task to cast.</param>
    /// <returns>Casted task.</returns>
    [Pure] public static Task<T> CastUnsafe<T>(this Task task)
        => Unsafe.As<Task, Task<T>>(ref task);

    /// <summary>
    /// Applies the specified <paramref name="converter"/> to the specified <paramref name="valueTask"/>.
    /// </summary>
    /// <typeparam name="TFrom">Return type of <paramref name="valueTask"/>.</typeparam>
    /// <typeparam name="TTo">Return type of result.</typeparam>
    /// <param name="valueTask">Task, to which <paramref name="converter"/> is applied.</param>
    /// <param name="converter">Converter to apply to <paramref name="valueTask"/>.</param>
    /// <returns></returns>
    [Pure] public static ValueTask<TTo> Transform<TFrom, TTo>(this ValueTask<TFrom> valueTask, [InstantHandle] Func<TFrom, TTo> converter)
    {
        return valueTask.IsCompletedSuccessfully ? new(converter(valueTask.Result)) : TransformSlow(valueTask, converter);

        static async ValueTask<TTo> TransformSlow(ValueTask<TFrom> task, Func<TFrom, TTo> converter)
            => converter(await task)!;
    }

}