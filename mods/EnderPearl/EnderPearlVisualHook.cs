using System;
using UnityEngine;
using ItemStatsSystem;

namespace EnderPearl;

internal sealed class EnderPearlVisualHook : MonoBehaviour
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

        // Ensure instance subscribes exactly once
        if (_subscribed)
        {
            return;
        }

        _subscribed = true;

        // Attach models from bundle when agents are created
        item.AgentUtilities.onCreateAgent += OnCreateAgent;

        // Also try agent-prefab injection (advanced) for this instance
        ModAssets.TryInjectItemAgents(item, modPath);

        ModLog.Info($"[EnderPearl] VisualHook enabled. modPath='{modPath}' itemInstance={item.GetInstanceID()} typeID={item.TypeID}");
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

            ModAssets.TryAttachModelToAgent(agent, modPath);
        }
        catch (Exception e)
        {
            ModLog.Warn($"[EnderPearl] OnCreateAgent failed: {e.Message}");
        }
    }
}
