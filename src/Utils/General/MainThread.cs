using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonoPlus.Utils.Collections;

namespace MonoPlus.Utils.General;

/// <summary>
/// Helper class for keeping track of <see cref="Tasks"/>, and throwing exceptions to main thread.
/// </summary>
public static class MainThread
{
    private static IndexedList<Task> Tasks = new();
    private static List<Exception> Exceptions = new(2);

    /// <summary>
    /// Updates all tasks managed by <see cref="MainThread"/>.
    /// </summary>
    /// <exception cref="AggregateException">One or more task threw an exception.</exception>
    public static void Update()
    {
        if (Exceptions.Count != 0) throw new AggregateException(Exceptions);
        
        if (Tasks.Count == 0) return;

        for (var i = 0; i < Tasks.Count; i++)
        {
            Task? task = Tasks[i];
            if (task is null || !task.IsCompleted) continue;
            if (task.Exception is not null) Exceptions.Add(task.Exception);
            Tasks.RemoveAt(i);
        }

        if (Exceptions.Count == 0) return;
        throw new AggregateException(Exceptions);
    }

    /// <summary>
    /// Adds the specified <paramref name="exception"/> to be thrown at main thread, to make try/catch catch it.
    /// </summary>
    /// <param name="exception">Exception to throw at main thread.</param>
    /// <exception cref="Exception">The <paramref name="exception"/>.</exception>
    public static void Add(Exception exception) => Exceptions.Add(exception);

    /// <summary>
    /// Adds the specified <paramref name="task"/> to the list of tasks managed by <see cref="MainThread"/>.
    /// </summary>
    /// <param name="task">Task to add.</param>
    public static void Add(Task task) => Tasks.Add(task);

    /// <summary>
    /// Adds the specified <paramref name="tasks"/> to the list of tasks managed by <see cref="MainThread"/>.
    /// </summary>
    /// <param name="tasks">Tasks to add.</param>
    public static void Add(IEnumerable<Task> tasks) => Tasks.AddRange(tasks);
}