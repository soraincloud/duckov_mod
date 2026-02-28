using System.Collections;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;

namespace TotemOfUndying;

internal sealed class TotemRescueSystem : MonoBehaviour
{
    private const float InvincibleSeconds = 3f;

    private static TotemRescueSystem? _instance;

    internal static void Initialize()
    {
        if (_instance != null)
        {
            return;
        }

        var go = new GameObject("TotemOfUndying_RescueSystem");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<TotemRescueSystem>();
    }

    internal static void Deinitialize()
    {
        if (_instance == null)
        {
            return;
        }

        Destroy(_instance.gameObject);
        _instance = null;
    }

    private void OnEnable()
    {
        Health.OnHurt += OnHealthHurt;
    }

    private void OnDisable()
    {
        Health.OnHurt -= OnHealthHurt;
    }

    private static void OnHealthHurt(Health health, DamageInfo damageInfo)
    {
        if (_instance == null || health == null || health.IsDead)
        {
            return;
        }

        if (health.CurrentHealth > 0f)
        {
            return;
        }

        if (!TryFindTotemInTotemSlot(health, out var totemItem))
        {
            return;
        }

        ConsumeTotem(totemItem);

        var healTarget = Mathf.Max(1f, health.MaxHealth * 0.5f);
        var wasInvincible = health.Invincible;
        health.SetHealth(healTarget);
        health.SetInvincible(true);

        _instance.StartCoroutine(DisableInvincibleAfterDelay(health, wasInvincible));

        var pos = health.transform.position + Vector3.up;
        var character = health.TryGetCharacter();
        if (character != null)
        {
            pos = character.transform.position + Vector3.up * 1.2f;
        }

        SpawnRescueParticles(pos);
    }

    private static bool TryFindTotemInTotemSlot(Health health, out Item item)
    {
        item = null!;

        var character = health.TryGetCharacter();
        if (character == null || character.CharacterItem == null || character.CharacterItem.Slots == null)
        {
            return false;
        }

        foreach (Slot slot in character.CharacterItem.Slots)
        {
            if (slot == null || slot.Content == null)
            {
                continue;
            }

            if (!IsTotemSlot(slot.Key))
            {
                continue;
            }

            if (slot.Content.TypeID != ModBehaviour.TotemOfUndyingTypeId)
            {
                continue;
            }

            item = slot.Content;
            return true;
        }

        return false;
    }

    private static bool IsTotemSlot(string? slotKey)
    {
        if (string.IsNullOrWhiteSpace(slotKey))
        {
            return false;
        }

        var key = slotKey.ToLowerInvariant();
        return key == "totem" || key.Contains("totem") || key == "soulcube" || key.Contains("soulcube");
    }

    private static void ConsumeTotem(Item totemItem)
    {
        if (totemItem.Stackable)
        {
            totemItem.StackCount--;
            return;
        }

        totemItem.Detach();
        totemItem.DestroyTree();
    }

    private static IEnumerator DisableInvincibleAfterDelay(Health health, bool wasInvincible)
    {
        yield return new WaitForSeconds(InvincibleSeconds);

        if (health == null || wasInvincible)
        {
            yield break;
        }

        health.SetInvincible(false);
    }

    private static void SpawnRescueParticles(Vector3 position)
    {
        var go = new GameObject("TotemOfUndying_RescueFx");
        go.transform.position = position;

        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.9f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.8f, 4.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 180;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 95)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.42f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(CreateRescueGradient());

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        ps.Play();
        Destroy(go, 2f);
    }

    private static Gradient CreateRescueGradient()
    {
        return new Gradient
        {
            colorKeys = new[]
            {
                new GradientColorKey(new Color(1f, 0.95f, 0.3f, 1f), 0f),
                new GradientColorKey(new Color(0.45f, 0.95f, 0.3f, 1f), 1f)
            },
            alphaKeys = new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        };
    }
}
