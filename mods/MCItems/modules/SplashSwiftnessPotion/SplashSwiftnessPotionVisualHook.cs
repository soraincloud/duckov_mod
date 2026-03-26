using System;
using ItemStatsSystem;
using UnityEngine;

namespace SplashSwiftnessPotion;

internal sealed class SplashSwiftnessPotionVisualHook : MonoBehaviour
{
    [SerializeField]
    private string? modPath;

    private bool _subscribed;

    internal void SetModPath(string? path)
    {
        modPath = path;
        ModLog.Initialize(path);
    }

    private void OnEnable()
    {
        ModLog.Initialize(modPath);

        var item = GetComponent<Item>();
        if (item == null || _subscribed)
        {
            return;
        }

        _subscribed = true;
        item.AgentUtilities.onCreateAgent += OnCreateAgent;
        ModAssets.TryInjectItemAgents(item, modPath);
    }

    private void OnDisable()
    {
        var item = GetComponent<Item>();
        if (item != null)
        {
            item.AgentUtilities.onCreateAgent -= OnCreateAgent;
        }

        _subscribed = false;
    }

    private void OnCreateAgent(Item master, ItemAgent agent)
    {
        try
        {
            if (agent != null)
            {
                ModAssets.TryAttachModelToAgent(agent, modPath);
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn($"[SplashSwiftnessPotion] OnCreateAgent failed: {exception.Message}");
        }
    }
}