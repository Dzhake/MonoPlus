using System;
using System.Collections.Generic;
using MonoPlus.AssetsSystem;

namespace Monod.AssetsSystem;

public struct AssetHandle : IAssetListener, IDisposable
{
    public Action<object> SetField;
    
    public AssetHandle(string path)
    {
        SetField = asset => field = asset;
        
    }
    
    public void ReloadAssets(HashSet<object>? oldAssets) => throw new NotImplementedException();
    
    public void Dispose()
    {
        
    }
}