using System;
using System.Collections.Generic;
using Duckov.Modding;
using UnityEngine;

namespace MCItems;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    private readonly List<Duckov.Modding.ModBehaviour> _modules = new();

    protected override void OnAfterSetup()
    {
        InitializeModule<global::EnderPearl.ModBehaviour>("EnderPearl", "末影珍珠");
        InitializeModule<global::GoldenApple.ModBehaviour>("GoldenApple", "金苹果");
        InitializeModule<global::SplashHealingPotion.ModBehaviour>("SplashHealingPotion", "治疗药水");
        InitializeModule<global::TotemOfUndying.ModBehaviour>("TotemOfUndying", "图腾");
    }

    protected override void OnBeforeDeactivate()
    {
        for (var index = _modules.Count - 1; index >= 0; index--)
        {
            var module = _modules[index];
            if (module == null)
            {
                continue;
            }

            try
            {
                module.NotifyBeforeDeactivate();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            try
            {
                Destroy(module.gameObject);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        _modules.Clear();
    }

    private void InitializeModule<TModule>(string name, string displayName) where TModule : Duckov.Modding.ModBehaviour
    {
        var moduleObject = new GameObject(name);
        moduleObject.transform.SetParent(transform, false);

        try
        {
            var module = moduleObject.AddComponent<TModule>();
            module.Setup(master, BuildModuleInfo(name, displayName));
            _modules.Add(module);
        }
        catch
        {
            Destroy(moduleObject);
            throw;
        }
    }

    private ModInfo BuildModuleInfo(string name, string displayName)
    {
        return new ModInfo
        {
            path = info.path,
            name = name,
            displayName = displayName,
            description = info.description,
            preview = info.preview,
            dllFound = info.dllFound,
            isSteamItem = info.isSteamItem,
            publishedFileId = info.publishedFileId
        };
    }
}
