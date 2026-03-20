using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace TotemOfUndying;

internal static class ModLog
{
    private static readonly object LockObj = new object();
    private static string? _logFilePath;
    private static bool _initialized;

    internal static void Initialize(string? modPath)
    {
        if (_initialized)
        {
            return;
        }

        try
        {
            // 1) Prefer writing to the mod folder (easy to find)
            if (!string.IsNullOrWhiteSpace(modPath))
            {
                var preferred = Path.Combine(modPath, "totemofundying.log");
                if (TrySetLogPath(preferred))
                {
                    WriteRaw($"=== TotemOfUndying log start {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n");
                    WriteLine($"logPath={_logFilePath}");
                    WriteLine($"persistentDataPath={Application.persistentDataPath}");
                    return;
                }
            }

            // 2) Fallback to persistentDataPath (always writable)
            var fallbackDir = Path.Combine(Application.persistentDataPath, "TotemOfUndying");
            var fallback = Path.Combine(fallbackDir, "totemofundying.log");
            if (TrySetLogPath(fallback))
            {
                WriteRaw($"=== TotemOfUndying log start {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n");
                WriteLine($"logPath={_logFilePath}");
                WriteLine($"modPath={(string.IsNullOrWhiteSpace(modPath) ? "<empty>" : modPath)}");
                WriteLine($"persistentDataPath={Application.persistentDataPath}");
                return;
            }

            // 3) Last resort: /tmp (macOS/Linux). This is mainly for debugging when other paths fail.
            var tmp = Path.Combine(Path.GetTempPath(), "totemofundying.log");
            if (TrySetLogPath(tmp))
            {
                WriteRaw($"=== TotemOfUndying log start {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n");
                WriteLine($"logPath={_logFilePath}");
                WriteLine($"modPath={(string.IsNullOrWhiteSpace(modPath) ? "<empty>" : modPath)}");
                WriteLine($"persistentDataPath={Application.persistentDataPath}");
            }
        }
        catch
        {
            // ignore
        }
        finally
        {
            _initialized = true;
        }
    }

    internal static void Info(string message)
    {
        Debug.Log(message);
        WriteLine(message);
    }

    internal static void Warn(string message)
    {
        Debug.LogWarning(message);
        WriteLine("[WARN] " + message);
    }

    internal static void Error(string message)
    {
        Debug.LogError(message);
        WriteLine("[ERROR] " + message);
    }

    internal static void Exception(Exception e)
    {
        Debug.LogException(e);
        WriteLine("[EXCEPTION] " + e);
    }

    private static void WriteLine(string message)
    {
        WriteRaw($"{DateTime.Now:HH:mm:ss.fff} {message}\n");
    }

    private static void WriteRaw(string text)
    {
        try
        {
            var path = _logFilePath;
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            lock (LockObj)
            {
                File.AppendAllText(path, text, Encoding.UTF8);
            }
        }
        catch
        {
            // ignore
        }
    }

    private static bool TrySetLogPath(string filePath)
    {
        try
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Probe write access
            File.AppendAllText(filePath, string.Empty, Encoding.UTF8);

            _logFilePath = filePath;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
