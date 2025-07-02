using Linguini.Bundle;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace MonoPlus.AssetsSystem;

/// <summary>
///   <para>Defines the types of assets.</para>
/// </summary>
public enum AssetType
{
    /// <summary>
    /// Unknown asset type, not supported by <see cref="MonoPlus.AssetsSystem"/>.
    /// </summary>
    Unknown,

    /// <summary>
    /// Audio asset type, converted into <see cref="SoundEffect"/>.
    /// </summary>
    Audio,

    /// <summary>
    /// Image asset type, converted into <see cref="Texture2D"/>.
    /// </summary>
    Image,

    /// <summary>
    /// Text asset type, converted into <see cref="string"/>.
    /// </summary>
    Text,

    /// <summary>
    /// Binary asset type, converted into <see cref="T:byte[]"/>.
    /// </summary>
    Binary,

    /// <summary>
    /// Effect asset type, converted into <see cref="Effect"/>.
    /// </summary>
    Effect,
}
