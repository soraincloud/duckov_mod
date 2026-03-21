using UnityEngine;

namespace MCPrerequisite;

/// <summary>
/// MC 前置的总入口，负责按生命周期协调材料注册、配方注入和分类过滤刷新。
/// </summary>
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
        // 先注册动态材料物品，再注册依赖这些材料的配方和分类过滤器。
        MaterialItemRegistry.Initialize(info.path);
        MCMaterialCraftingService.Initialize();
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
        MCMaterialCraftingService.Deinitialize();
        MaterialItemRegistry.Deinitialize();
        _initialized = false;
    }

    private void Update()
    {
        if (_initialized)
        {
            // 运行时持续兜底，处理场景切换后才出现的箱子、背包和工作台视图。
            MaterialItemRegistry.UpdateRuntimeState();
            MCCategoryService.UpdateRuntimeState();
        }
    }
}