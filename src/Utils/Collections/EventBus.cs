using System;
using System.Collections.Generic;


namespace Monod.Utils.Collections;

/// <summary>
/// Class for storing and invoking a bunch of <see cref="Action"/>s.
/// </summary>
public class EventBus
{
    /// <summary>
    /// All delegates that should be invoked when <see cref="Emit"/> is called.
    /// </summary>
    private readonly HashSet<Action> Subscribers = new();
    
    /// <summary>
    /// Add the specified <paramref name="subscriber"/> to a collection of delegates that will be called on <see cref="Emit"/>. 
    /// </summary>
    /// <param name="subscriber"><see cref="Action"/> delegate that will be called on <see cref="Emit"/>.</param>
    public void Subscribe(Action subscriber) => Subscribers.Add(subscriber);

    /// <summary>
    /// Remove the specified <paramref name="subscriber"/> from a collection of delegates that will be called on <see cref="Emit"/>. 
    /// </summary>
    /// <param name="subscriber"><see cref="Action"/> delegate that won't be called on <see cref="Emit"/> anymore.</param>
    public void Unsubscribe(Action subscriber) => Subscribers.Remove(subscriber);

    /// <summary>
    /// Invoke all subscribed delegates.
    /// </summary>
    public void Emit()
    {
        foreach (Action subscriber in Subscribers) subscriber();
    }

    /// <summary>
    /// Add the specified <paramref name="subscriber"/> to a collection of delegates that will be called on <see cref="Emit"/>. 
    /// </summary>
    /// <param name="eventBus">Self.</param>
    /// <param name="subscriber"><see cref="Action"/> delegate that will be called on <see cref="Emit"/>.</param>
    /// <remarks>Short version of <see cref="Subscribe"/>.</remarks>
    public static EventBus operator +(EventBus eventBus, Action subscriber)
    {
        eventBus.Subscribe(subscriber);
        return eventBus;
    }

    /// <summary>
    /// Remove the specified <paramref name="subscriber"/> from a collection of delegates that will be called on <see cref="Emit"/>. 
    /// </summary>
    /// <param name="eventBus">Self.</param>
    /// <param name="subscriber"><see cref="Action"/> delegate that won't be called on <see cref="Emit"/> anymore.</param>
    /// <remarks>Short version of <see cref="Unsubscribe"/>.</remarks>
    public static EventBus operator -(EventBus eventBus, Action subscriber)
    {
        eventBus.Unsubscribe(subscriber);
        return eventBus;
    }  
}