using UnityEngine;

namespace MCPrerequisite;

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
        MaterialItemRegistry.Initialize(info.path);
        MCCategoryService.Initialize(info.path);
        Debug.Log("[MCPrerequisite] Loaded.");
    }

    protected override void OnBeforeDeactivate()
    {
        if (!_initialized)
        {
            return;
        }

        MCCategoryService.Deinitialize();
        MaterialItemRegistry.Deinitialize();
        _initialized = false;
    }

    private void Update()
    {
        if (_initialized)
        {
            MaterialItemRegistry.UpdateRuntimeState();
            MCCategoryService.UpdateRuntimeState();
        }
    }
}