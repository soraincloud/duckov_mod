using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using FMOD;
using FMODUnity;

namespace EnderPearl;

internal static class ModSfx
{
	private const string ThrowWavName = "throw.wav";
	private const string Transmit1WavName = "transmit1.wav";
	private const string Transmit2WavName = "transmit2.wav";

	private static string? _modPath;
	private static Runner? _runner;
	private static bool _verbose;

	private static readonly Dictionary<string, Sound> FmodWavSounds = new Dictionary<string, Sound>(StringComparer.OrdinalIgnoreCase);

	private static bool _initialized;

	internal static void Initialize(string? modPath)
	{
		if (_initialized) return;
		if (string.IsNullOrWhiteSpace(modPath))
		{
			ModLog.Warn("[EnderPearl] ModSfx.Initialize skipped: modPath is null/empty");
			return;
		}

		_initialized = true;
		_modPath = modPath;
		_verbose = File.Exists(Path.Combine(modPath, "assets", "sfx", "verbose_sfx_log.txt"));

		if (_verbose) ModLog.Info($"[EnderPearl] ModSfx init. modPath='{modPath}'");

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

	internal static void PlayThrow(Vector3 position)
	{
		if (!_initialized) return;
		if (_verbose) ModLog.Info($"[EnderPearl] SFX throw at {position}");

		// Next best: FMOD core plays WAV directly (works even if Unity Audio is disabled in-game).
		if (TryPlayFmodWav(ModAssetPath("assets", "sfx", ThrowWavName), volume: 1f))
		{
			return;
		}
	}

	internal static void PlayTransmit(Vector3 position)
	{
		if (!_initialized) return;
		if (_verbose) ModLog.Info($"[EnderPearl] SFX transmit at {position}");

		// Randomly pick one of the two landing/teleport sounds.
		var useFirst = UnityEngine.Random.Range(0, 2) == 0;

		var wavName = useFirst ? Transmit1WavName : Transmit2WavName;
		if (TryPlayFmodWav(ModAssetPath("assets", "sfx", wavName), volume: 1f))
		{
			return;
		}
	}

	private static string ModAssetPath(params string[] parts)
	{
		var basePath = _modPath ?? string.Empty;
		return Path.Combine(new[] { basePath }.Concat(parts).ToArray());
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
				// If called very early, try playing once FMOD becomes ready.
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
				var rCreate = core.createSound(wavPath, MODE._2D | MODE.LOOP_OFF, out sound);
				if (rCreate != RESULT.OK)
				{
					ModLog.Warn($"[EnderPearl] FMOD createSound failed: {rCreate} ({Error.String(rCreate)}) path='{wavPath}'");
					return false;
				}
				FmodWavSounds[wavPath] = sound;
				ModLog.Info($"[EnderPearl] FMOD WAV loaded: {Path.GetFileName(wavPath)}");
			}

			var rPlay = core.playSound(sound, default(ChannelGroup), true, out Channel channel);
			if (rPlay != RESULT.OK)
			{
				ModLog.Warn($"[EnderPearl] FMOD playSound failed: {rPlay} ({Error.String(rPlay)})");
				return false;
			}

			// 2D playback: just set volume and unpause.
			channel.setVolume(Mathf.Clamp01(volume));
			channel.setPaused(false);
			return true;
		}
		catch (Exception e)
		{
			ModLog.Warn($"[EnderPearl] FMOD WAV play exception: {e.GetType().Name}: {e.Message}");
			return false;
		}
	}

	private static void TryDeferredPlayFmodWav(string wavPath, float volume)
	{
		try
		{
			if (_runner == null) return;
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
			if (_verbose) ModLog.Warn($"[EnderPearl] FMOD not initialized; skipped deferred WAV play: '{Path.GetFileName(wavPath)}'");
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
		if (_runner != null) return;

		var go = new GameObject("EnderPearl_Sfx");
		UnityEngine.Object.DontDestroyOnLoad(go);
		_runner = go.AddComponent<Runner>();
	}

	private sealed class Runner : MonoBehaviour
	{ }
}
