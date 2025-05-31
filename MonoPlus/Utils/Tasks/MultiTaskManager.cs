using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonoPlus.Utils.Tasks;

/// <summary>
/// Small helper class for running, storing and keeping track of multiply similar tasks. (E.g.: same method with different parameters.)
/// </summary>
/// <remarks>
/// The class is designed to be created once running tasks, and deleted once all tasks are finished. <see cref="InProgress"/> is also false before adding any task, so you either need to track whether tasks were added, or create <see cref="MultiTaskManager"/> only when needed.
/// </remarks>
public class MultiTaskManager
{
    /// <summary>
    /// List of tasks. If Count is higher than 0, then some tasks are in progress.
    /// </summary>
    public List<Task> Tasks = new();

    /// <summary>
    /// Whether any task is still in progress, and was not removed from the <see cref="Tasks"/> in <see cref="Update"/>.
    /// </summary>
    public bool InProgress => Tasks.Count > 0;

    /// <summary>
    /// Removes all finished tasks from <see cref="Tasks"/>, and throws exceptions from tasks which threw an exception.
    /// </summary>
    public void Update()
    {
        for (int i = 0; i < Tasks.Count; i++)
        {
            Task task = Tasks[i];

            if (!task.IsCompleted) continue;

            if (task.Exception is not null) throw task.Exception;
            OnTaskFinish(i);
            i--;
        }
    }

    /// <summary>
    /// Called for finished task in <see cref="Update"/> if task didn't throw an exception.
    /// </summary>
    /// <param name="index">Index of the task in <see cref="Tasks"/>.</param>
    /// <remarks><see langword="base"/> of this method removes task at <paramref name="index"/> from <see cref="Tasks"/>. You probably want to call it as late as possible.</remarks>
    public virtual void OnTaskFinish(int index)
    {
        Tasks.RemoveAt(index);
    }

    /// <summary>
    /// Adds the task to the <see cref="Tasks"/>.
    /// </summary>
    /// <param name="task">Task to add.</param>
    public void Add(Task task)
    {
        Tasks.Add(task);
    }
}


/// <summary>
/// Small helper class for running, storing and keeping track of multiply similar tasks, all of which have return type of <see cref="TReturn"/>. (E.g.: same method with different parameters.)
/// </summary>
/// <remarks>
/// The class is designed to be created once running tasks, and deleted once all tasks are finished. <see cref="MultiTaskManager.InProgress"/> is also false before adding any task, so you either need to track whether tasks were added, or create <see cref="MultiTaskManager"/> only when needed.
/// </remarks>
public class MultiTaskManager<TReturn> : MultiTaskManager
{
    /// <summary>
    /// List of objects already finished tasks returned.
    /// </summary>
    public List<TReturn> Results = new();

    /// <inheritdoc/> 
    public override void OnTaskFinish(int index)
    {
        Task task = Tasks[index];
        if (task is not Task<TReturn> returnTask)
            throw new InvalidOperationException($"Task at index {index} could not be casted to Task<{typeof(TReturn)}>");
        Results.Add(returnTask.Result);
        base.OnTaskFinish(index);
    }

    ///<inheritdoc cref="MultiTaskManager.Add"/>  
    public void Add(Task<TReturn> task)
    {
        Tasks.Add(task);
    }
}