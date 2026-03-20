using System;
using ItemStatsSystem;
using UnityEngine;

namespace TotemOfUndying;

internal sealed class TotemVisualHook : MonoBehaviour
{
    [SerializeField]
    private string? modPath;

    private bool _subscribed;

    internal void SetModPath(string? path)
    {
        modPath = path;
        ModLog.Initialize(modPath);
    }

    private void OnEnable()
    {
        ModLog.Initialize(modPath);

        var item = GetComponent<Item>();
        if (item == null)
        {
            return;
        }

        if (_subscribed)
        {
            return;
        }

        _subscribed = true;

        item.AgentUtilities.onCreateAgent += OnCreateAgent;
        TotemModelAssets.TryInjectItemAgents(item, modPath);

        ModLog.Info($"[TotemOfUndying] VisualHook enabled. itemInstance={item.GetInstanceID()} typeID={item.TypeID} modPath='{modPath}'");
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
            if (agent == null)
            {
                return;
            }

            TotemModelAssets.TryAttachModelToAgent(agent, modPath);
        }
        catch (Exception e)
        {
            ModLog.Warn($"[TotemOfUndying] OnCreateAgent failed: {e.Message}");
        }
    }
}
