using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace SplashSwiftnessPotion;

internal static class ModLog
{
    private static readonly object LockObj = new();
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
            if (!string.IsNullOrWhiteSpace(modPath))
            {
                var preferred = Path.Combine(modPath, "splashswiftnesspotion.log");
                if (TrySetLogPath(preferred))
                {
                    WriteRaw($"=== SplashSwiftnessPotion log start {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n");
                    return;
                }
            }

            var fallbackDir = Path.Combine(Application.persistentDataPath, "SplashSwiftnessPotion");
            var fallback = Path.Combine(fallbackDir, "splashswiftnesspotion.log");
            if (TrySetLogPath(fallback))
            {
                WriteRaw($"=== SplashSwiftnessPotion log start {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n");
                return;
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

    internal static void Exception(Exception exception)
    {
        Debug.LogException(exception);
        WriteLine("[EXCEPTION] " + exception);
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