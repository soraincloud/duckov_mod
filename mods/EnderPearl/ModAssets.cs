using System;
using System.Collections.Generic;
using System.IO;
using ItemStatsSystem;
using UnityEngine;

namespace EnderPearl;

internal static class ModAssets
{
    private const string IconFileName = "icon.png";

    // Bundle file names we will try under the mod folder
    private static readonly string[] BundleCandidateNames =
    {
        "enderpearl_assets",
        "enderpearl_assets.bundle",
        "enderpearl_assets.unity3d"
    };

    private const string HandheldAgentPrefabName = "EnderPearl_HandheldAgent";
    private const string PickupAgentPrefabName = "EnderPearl_PickupAgent";

    // Simpler workflow: plain model prefabs (no game scripts required)
    private const string HandheldModelPrefabName = "EnderPearl_HandheldModel";
    private const string PickupModelPrefabName = "EnderPearl_PickupModel";

    private static AssetBundle? _bundle;

    internal static Sprite? TryLoadIconSprite(string? modPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(modPath))
            {
                return null;
            }

            var iconPath = Path.Combine(modPath, IconFileName);
            if (!File.Exists(iconPath))
            {
                return null;
            }

            var pngBytes = File.ReadAllBytes(iconPath);
            if (pngBytes.Length == 0)
            {
                return null;
            }

            // Texture size will be replaced by LoadImage
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
            if (!ImageConversion.LoadImage(texture, pngBytes))
            {
                return null;
            }

            texture.name = "EnderPearl_Icon";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            var rect = new Rect(0, 0, texture.width, texture.height);
            var pivot = new Vector2(0.5f, 0.5f);

            // Using pixelsPerUnit=100 is a common Unity default; UI will scale anyway.
            return Sprite.Create(texture, rect, pivot, pixelsPerUnit: 100f);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[EnderPearl] Failed to load icon.png: {e.Message}");
            return null;
        }
    }

    internal static void TryInjectItemAgents(Item item, string? modPath)
    {
        try
        {
            if (item == null || string.IsNullOrWhiteSpace(modPath))
            {
                return;
            }

            var bundle = TryLoadBundle(modPath);
            if (bundle == null)
            {
                return;
            }

            var handheldPrefab = bundle.LoadAsset<ItemAgent>(HandheldAgentPrefabName);
            var pickupPrefab = bundle.LoadAsset<ItemAgent>(PickupAgentPrefabName);

            if (handheldPrefab == null && pickupPrefab == null)
            {
                return;
            }

            UpsertAgentPrefab(item.AgentUtilities, "Handheld", handheldPrefab);
            UpsertAgentPrefab(item.AgentUtilities, "Pickup", pickupPrefab);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[EnderPearl] Failed to inject item agents: {e.Message}");
        }
    }

    internal static void TryAttachModelsOnAgentCreate(Item item, string? modPath)
    {
        try
        {
            if (item == null || string.IsNullOrWhiteSpace(modPath))
            {
                return;
            }

            // Avoid double-subscription if prefab creation is called more than once
            item.AgentUtilities.onCreateAgent -= OnCreateAgentAttachModel;
            item.AgentUtilities.onCreateAgent += OnCreateAgentAttachModel;

            void OnCreateAgentAttachModel(Item master, ItemAgent agent)
            {
                try
                {
                    if (agent == null)
                    {
                        return;
                    }

                    var bundle = TryLoadBundle(modPath);
                    if (bundle == null)
                    {
                        return;
                    }

                    GameObject? modelPrefab = null;
                    switch (agent.AgentType)
                    {
                        case ItemAgent.AgentTypes.handheld:
                            modelPrefab = bundle.LoadAsset<GameObject>(HandheldModelPrefabName);
                            break;
                        case ItemAgent.AgentTypes.pickUp:
                            modelPrefab = bundle.LoadAsset<GameObject>(PickupModelPrefabName);
                            break;
                        default:
                            return;
                    }

                    if (modelPrefab == null)
                    {
                        return;
                    }

                    // Clean up previously attached model if any
                    var existing = agent.transform.Find(modelPrefab.name);
                    if (existing != null)
                    {
                        UnityEngine.Object.Destroy(existing.gameObject);
                    }

                    var instance = UnityEngine.Object.Instantiate(modelPrefab, agent.transform);
                    instance.name = modelPrefab.name;
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localRotation = Quaternion.identity;
                    instance.transform.localScale = Vector3.one;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[EnderPearl] Failed to attach model prefab: {e.Message}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[EnderPearl] Failed to hook onCreateAgent: {e.Message}");
        }
    }

    private static AssetBundle? TryLoadBundle(string modPath)
    {
        if (_bundle != null)
        {
            return _bundle;
        }

        foreach (var name in BundleCandidateNames)
        {
            var bundlePath = Path.Combine(modPath, name);
            if (!File.Exists(bundlePath))
            {
                continue;
            }

            try
            {
                _bundle = AssetBundle.LoadFromFile(bundlePath);
                if (_bundle != null)
                {
                    Debug.Log($"[EnderPearl] Loaded AssetBundle: {bundlePath}");
                    return _bundle;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[EnderPearl] Failed to load AssetBundle '{bundlePath}': {e.Message}");
            }
        }

        return null;
    }

    private static void UpsertAgentPrefab(ItemAgentUtilities agentUtilities, string key, ItemAgent? prefab)
    {
        if (agentUtilities == null || prefab == null)
        {
            return;
        }

        var agents = ReflectionUtil.GetPrivateField<List<ItemAgentUtilities.AgentKeyPair>>(agentUtilities, "agents");
        if (agents == null)
        {
            agents = new List<ItemAgentUtilities.AgentKeyPair>();
            ReflectionUtil.SetPrivateField(agentUtilities, "agents", agents);
        }

        // Remove existing entry with same key (if any)
        agents.RemoveAll(p => p != null && string.Equals(p.key, key, StringComparison.Ordinal));

        agents.Add(new ItemAgentUtilities.AgentKeyPair
        {
            key = key,
            agentPrefab = prefab
        });

        // Clear hashed cache so the new entry is picked up
        ReflectionUtil.SetPrivateField(agentUtilities, "hashedAgentsCache", null);
    }
}
