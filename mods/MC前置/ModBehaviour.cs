using UnityEngine;

namespace MC前置;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    private static bool _initialized;

    protected override void OnAfterSetup()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        MCCategoryService.Initialize(info.path);
        Debug.Log("[MC前置] Loaded.");
    }

    protected override void OnBeforeDeactivate()
    {
        if (!_initialized)
        {
            return;
        }

        MCCategoryService.Deinitialize();
        _initialized = false;
    }
}