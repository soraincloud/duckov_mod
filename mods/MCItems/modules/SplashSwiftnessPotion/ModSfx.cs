using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FMOD;
using FMODUnity;
using UnityEngine;

namespace SplashSwiftnessPotion;

internal static class ModSfx
{
    private const string ThrowWavName = "splashhealingpotion_throw.wav";

    private static string? _modPath;
    private static Runner? _runner;
    private static string[] _glassBreakWavPaths = Array.Empty<string>();
    private static bool _initialized;

    private static readonly Dictionary<string, Sound> FmodWavSounds = new(StringComparer.OrdinalIgnoreCase);

    internal static void Initialize(string? modPath)
    {
        if (_initialized || string.IsNullOrWhiteSpace(modPath))
        {
            return;
        }

        _initialized = true;
        _modPath = modPath;
        _glassBreakWavPaths = FindGlassBreakWavs(modPath);
        EnsureRunner();
    }

    internal static void Deinitialize()
    {
        _initialized = false;
        _modPath = null;
        _glassBreakWavPaths = Array.Empty<string>();

        foreach (var sound in FmodWavSounds.Values)
        {
            try
            {
                if (sound.hasHandle())
                {
                    sound.release();
                }
            }
            catch
            {
                // ignore
            }
        }

        FmodWavSounds.Clear();

        if (_runner != null)
        {
            try
            {
                UnityEngine.Object.Destroy(_runner.gameObject);
            }
            catch
            {
                // ignore
            }

            _runner = null;
        }
    }

    internal static void PlayGlassBreak(Vector3 position)
    {
        if (!_initialized)
        {
            return;
        }

        var wavPath = GetRandomGlassBreakWavPath();
        if (!string.IsNullOrWhiteSpace(wavPath))
        {
            TryPlayFmodWav(wavPath, 1f);
        }
    }

    internal static void PlayThrow(Vector3 position)
    {
        if (!_initialized || string.IsNullOrWhiteSpace(_modPath))
        {
            return;
        }

        TryPlayFmodWav(Path.Combine(_modPath, "assets", "sfx", ThrowWavName), 1f);
    }

    private static string? GetRandomGlassBreakWavPath()
    {
        if (_glassBreakWavPaths.Length == 0)
        {
            return null;
        }

        return _glassBreakWavPaths[UnityEngine.Random.Range(0, _glassBreakWavPaths.Length)];
    }

    private static string[] FindGlassBreakWavs(string modPath)
    {
        try
        {
            var sfxDir = Path.Combine(modPath, "assets", "sfx");
            if (!Directory.Exists(sfxDir))
            {
                return Array.Empty<string>();
            }

            return Directory
                .GetFiles(sfxDir)
                .Where(path => string.Equals(Path.GetExtension(path), ".wav", StringComparison.OrdinalIgnoreCase))
                .Where(path => Path.GetFileNameWithoutExtension(path).StartsWith("glassBreak", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch (Exception exception)
        {
            ModLog.Warn($"[SplashSwiftnessPotion] Failed to enumerate glassBreak wavs: {exception.Message}");
            return Array.Empty<string>();
        }
    }

    private static bool TryPlayFmodWav(string wavPath, float volume)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(wavPath) || !File.Exists(wavPath))
            {
                return false;
            }

            if (!RuntimeManager.IsInitialized)
            {
                TryDeferredPlayFmodWav(wavPath, volume);
                return true;
            }

            var core = RuntimeManager.CoreSystem;
            if (core.handle == IntPtr.Zero)
            {
                return false;
            }

            if (!FmodWavSounds.TryGetValue(wavPath, out var sound) || !sound.hasHandle())
            {
                var createResult = core.createSound(wavPath, MODE._2D | MODE.LOOP_OFF, out sound);
                if (createResult != RESULT.OK)
                {
                    return false;
                }

                FmodWavSounds[wavPath] = sound;
            }

            var playResult = core.playSound(sound, default(ChannelGroup), true, out Channel channel);
            if (playResult != RESULT.OK)
            {
                return false;
            }

            channel.setVolume(Mathf.Clamp01(volume));
            channel.setPaused(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void TryDeferredPlayFmodWav(string wavPath, float volume)
    {
        if (_runner == null)
        {
            return;
        }

        _runner.StartCoroutine(DeferredPlayFmodWav(wavPath, volume));
    }

    private static IEnumerator DeferredPlayFmodWav(string wavPath, float volume)
    {
        var deadline = Time.realtimeSinceStartup + 2f;
        while (!RuntimeManager.IsInitialized && Time.realtimeSinceStartup < deadline)
        {
            yield return null;
        }

        if (RuntimeManager.IsInitialized)
        {
            TryPlayFmodWav(wavPath, volume);
        }
    }

    private static void EnsureRunner()
    {
        if (_runner != null)
        {
            return;
        }

        var go = new GameObject("SplashSwiftnessPotion_Sfx");
        UnityEngine.Object.DontDestroyOnLoad(go);
        _runner = go.AddComponent<Runner>();
    }

    private sealed class Runner : MonoBehaviour
    {
    }
}