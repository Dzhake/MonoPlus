using Microsoft.Xna.Framework;

namespace MonoPlus.Graphics;

/// <summary>
/// Extesions for <see cref="Point"/> class.
/// </summary>
public static class PointExtensions
{
    /// <summary>
    /// Converts the specified <paramref name="point"/> to <see cref="Vector2"/> with same values.
    /// </summary>
    /// <param name="point"><see cref="Point"/> to convert.</param>
    /// <returns><see cref="Vector2"/> representation of <paramref name="point"/>.</returns>
    public static Vector2 ToVector2(this Point point) => new(point.X, point.Y);
}
