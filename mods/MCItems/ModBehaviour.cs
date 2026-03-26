using System;
using System.Collections.Generic;
using Duckov.Modding;
using UnityEngine;

namespace MCItems;

/// <summary>
/// MCItems 本体只负责装配四个子模组，把同一包内的功能拆成独立生命周期单元。
/// </summary>
public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    private readonly List<Duckov.Modding.ModBehaviour> _modules = new();

    protected override void OnAfterSetup()
    {
        // 每个子模块都复用当前 ModInfo 的公共资源路径，但保留各自独立的内部名字和显示名。
        InitializeModule<global::EnderPearl.ModBehaviour>("EnderPearl", "末影珍珠");
        InitializeModule<global::GoldenApple.ModBehaviour>("GoldenApple", "金苹果");
        InitializeModule<global::SplashHealingPotion.ModBehaviour>("SplashHealingPotion", "治疗药水");
        InitializeModule<global::SplashSwiftnessPotion.ModBehaviour>("SplashSwiftnessPotion", "迅捷药水");
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
            // 手动调用 Setup，把总包的上下文传给子模块，避免它们依赖单独 dll 的加载入口。
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
