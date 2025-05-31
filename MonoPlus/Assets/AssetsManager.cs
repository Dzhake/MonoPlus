using System;
using System.Collections.Generic;
using MonoPlus.Utils.Collections;


namespace MonoPlus.Assets;

public class AssetsManager
{
    protected Dictionary<string, object> assets;

    protected IndexedList<Action<object[]>> listeners;
}
