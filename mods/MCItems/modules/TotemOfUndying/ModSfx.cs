using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FMOD;
using FMODUnity;
using UnityEngine;

namespace TotemOfUndying;

internal static class ModSfx
{
    private const string PreferredWavName = "totem.wav";

    private static string? _modPath;
    private static Runner? _runner;
    private static bool _verbose;
    private static bool _initialized;

    private static readonly Dictionary<string, Sound> FmodWavSounds = new(StringComparer.OrdinalIgnoreCase);

    internal static void Initialize(string? modPath)
    {
        if (_initialized)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(modPath))
        {
            ModLog.Warn("[TotemOfUndying] ModSfx.Initialize skipped: modPath is null/empty");
            return;
        }

        _initialized = true;
        _modPath = modPath;
        _verbose = File.Exists(Path.Combine(modPath, "assets", "sfx", "verbose_sfx_log.txt"));

        if (_verbose)
        {
            ModLog.Info($"[TotemOfUndying] ModSfx init. modPath='{modPath}'");
        }

        EnsureRunner();
    }

    internal static void Deinitialize()
    {
        _initialized = false;
        _modPath = null;
        _verbose = false;

        TryReleaseFmodWavSounds();

        try
        {
            if (_runner != null)
            {
                UnityEngine.Object.Destroy(_runner.gameObject);
            }
        }
        catch
        {
            // ignore
        }
        finally
        {
            _runner = null;
        }
    }

    internal static void PlayRescue(Vector3 position)
    {
        if (!_initialized)
        {
            return;
        }

        if (_verbose)
        {
            ModLog.Info($"[TotemOfUndying] SFX rescue at {position}");
        }

        var wavPath = ResolveRescueWavPath();
        if (wavPath == null)
        {
            if (_verbose)
            {
                ModLog.Warn("[TotemOfUndying] No rescue WAV found under assets/sfx");
            }
            return;
        }

        TryPlayFmodWav(wavPath, volume: 1f);
    }

    private static string? ResolveRescueWavPath()
    {
        if (string.IsNullOrWhiteSpace(_modPath))
        {
            return null;
        }

        var sfxDir = Path.Combine(_modPath, "assets", "sfx");
        if (!Directory.Exists(sfxDir))
        {
            return null;
        }

        var preferredPath = Directory.EnumerateFiles(sfxDir)
            .FirstOrDefault(path => string.Equals(Path.GetFileName(path), PreferredWavName, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(preferredPath))
        {
            return preferredPath;
        }

        return Directory.EnumerateFiles(sfxDir)
            .FirstOrDefault(path => string.Equals(Path.GetExtension(path), ".wav", StringComparison.OrdinalIgnoreCase));
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
                    ModLog.Warn($"[TotemOfUndying] FMOD createSound failed: {createResult} ({Error.String(createResult)}) path='{wavPath}'");
                    return false;
                }

                FmodWavSounds[wavPath] = sound;
                ModLog.Info($"[TotemOfUndying] FMOD WAV loaded: {Path.GetFileName(wavPath)}");
            }

            var playResult = core.playSound(sound, default(ChannelGroup), true, out Channel channel);
            if (playResult != RESULT.OK)
            {
                ModLog.Warn($"[TotemOfUndying] FMOD playSound failed: {playResult} ({Error.String(playResult)})");
                return false;
            }

            channel.setVolume(Mathf.Clamp01(volume));
            channel.setPaused(false);
            return true;
        }
        catch (Exception e)
        {
            ModLog.Warn($"[TotemOfUndying] FMOD WAV play exception: {e.GetType().Name}: {e.Message}");
            return false;
        }
    }

    private static void TryDeferredPlayFmodWav(string wavPath, float volume)
    {
        try
        {
            if (_runner == null)
            {
                return;
            }

            _runner.StartCoroutine(DeferredPlayFmodWav(wavPath, volume));
        }
        catch
        {
            // ignore
        }
    }

    private static IEnumerator DeferredPlayFmodWav(string wavPath, float volume)
    {
        var deadline = Time.realtimeSinceStartup + 2f;
        while (!RuntimeManager.IsInitialized && Time.realtimeSinceStartup < deadline)
        {
            yield return null;
        }

        if (!RuntimeManager.IsInitialized)
        {
            if (_verbose)
            {
                ModLog.Warn($"[TotemOfUndying] FMOD not initialized; skipped deferred WAV play: '{Path.GetFileName(wavPath)}'");
            }

            yield break;
        }

        TryPlayFmodWav(wavPath, volume);
    }

    private static void TryReleaseFmodWavSounds()
    {
        try
        {
            foreach (var kv in FmodWavSounds)
            {
                try
                {
                    if (kv.Value.hasHandle())
                    {
                        kv.Value.release();
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }
        catch
        {
            // ignore
        }
        finally
        {
            FmodWavSounds.Clear();
        }
    }

    private static void EnsureRunner()
    {
        if (_runner != null)
        {
            return;
        }

        var go = new GameObject("TotemOfUndying_Sfx");
        UnityEngine.Object.DontDestroyOnLoad(go);
        _runner = go.AddComponent<Runner>();
    }

    private sealed class Runner : MonoBehaviour
    {
    }
}