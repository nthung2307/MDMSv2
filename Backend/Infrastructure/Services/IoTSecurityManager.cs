using System;
using System.Security.Cryptography;
using System.Threading;

namespace Infrastructure.Services;

public static class IoTSecurityManager
{
    private static readonly object _lock = new();
    private static volatile bool _isInitialized = false;
    private static string _apiKey = string.Empty;
    private static string _allowedIps = string.Empty;
    private static bool _requireApiKey = false;
    private static DateTime _lastLoaded = DateTime.MinValue;

    // Telemetry
    public static DateTimeOffset? LastPushTime { get; private set; }
    public static string? LastPushIp { get; private set; }
    public static string? LastPushStatus { get; private set; }
    public static int LastPushTagCount { get; private set; }
    private static long _totalPushCount = 0;
    public static long TotalPushCount => Interlocked.Read(ref _totalPushCount);

    public static void RecordPush(string? ip, bool authorized, int tagCount)
    {
        LastPushTime = DateTimeOffset.Now;
        LastPushIp = ip ?? "Unknown";
        LastPushStatus = authorized ? "SUCCESS" : "UNAUTHORIZED";
        LastPushTagCount = tagCount;
        Interlocked.Increment(ref _totalPushCount);
    }

    public static bool IsInitialized => _isInitialized && (DateTime.UtcNow - _lastLoaded).TotalMinutes < 5;

    public static (string ApiKey, string AllowedIps, bool RequireApiKey) GetCachedConfig()
    {
        lock (_lock)
        {
            return (_apiKey, _allowedIps, _requireApiKey);
        }
    }

    public static void UpdateConfig(string apiKey, string allowedIps, bool requireApiKey)
    {
        lock (_lock)
        {
            _apiKey = apiKey ?? string.Empty;
            _allowedIps = allowedIps ?? string.Empty;
            _requireApiKey = requireApiKey;
            _isInitialized = true;
            _lastLoaded = DateTime.UtcNow;
        }
    }

    public static void Invalidate()
    {
        lock (_lock)
        {
            _isInitialized = false;
        }
    }

    public static string GenerateSecureKey()
    {
        var bytes = new byte[24];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var base64 = Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
        return $"MDMS_{base64}";
    }
}

