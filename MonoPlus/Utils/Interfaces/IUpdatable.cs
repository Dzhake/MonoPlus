namespace MonoPlus.Utils.Interfaces;

/// <summary>
/// Represents an object which has a state which needs to be updated regularly.
/// </summary>
public interface IUpdatable
{
    /// <summary>
    /// Updates type's state.
    /// </summary>
    public void Update();
}
