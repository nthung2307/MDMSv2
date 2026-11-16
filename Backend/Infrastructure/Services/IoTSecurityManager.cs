using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Infrastructure.Services.EmailProviders;

namespace Infrastructure.Services;

public sealed record UnmatchedSerialEntry(
    string MeterSerial,
    string RemoteIp,
    DateTimeOffset FirstSeenAt,
    DateTimeOffset LastSeenAt,
    long HitCount);

public sealed record SerialConflictEntry(
    string MeterSerial,
    string PrimaryIp,
    string OffendingIp,
    string Reason,
    DateTimeOffset LastDetectedAt,
    long ConflictCount);

public static class IoTSecurityManager
{
    private const int MaxUnmatchedSerialEntries = 50;
    private const int MaxConflictEntries = 50;
    private static readonly TimeSpan SessionIpLockDuration = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromSeconds(10);
    private const int MaxRequestsPerSerialWindow = 20;
    private const double DuplicateFullFrameSuppressMs = 1500;

    private sealed class SerialSessionState
    {
        public readonly object SyncRoot = new();
        public string PrimaryIp = string.Empty;
        public DateTimeOffset LastAcceptedAt = DateTimeOffset.MinValue;
        public DateTimeOffset LastFullFrameAt = DateTimeOffset.MinValue;
        public int LastFullFrameTagCount;
        public DateTimeOffset WindowStart = DateTimeOffset.MinValue;
        public int WindowHitCount;
    }

    private static readonly object _lock = new();
    public static readonly SemaphoreSlim ConfigRefreshLock = new(1, 1);
    private static volatile bool _isInitialized = false;
    private static string _apiKey = string.Empty;
    private static string _allowedIps = string.Empty;
    private static bool _requireApiKey = false;
    private static bool _allowLegacyTagRouting = true;
    private static DateTime _lastLoaded = DateTime.MinValue;
    private static long _lastTelemetryBroadcastTicks = 0;

    private static readonly Dictionary<string, UnmatchedSerialEntry> _unmatchedSerials = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, SerialSessionState> _serialSessions = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, SerialConflictEntry> _serialConflicts = new(StringComparer.OrdinalIgnoreCase);

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

    public static bool TryClaimTelemetryBroadcastSlot(TimeSpan minInterval)
    {
        var nowTicks = DateTimeOffset.UtcNow.UtcTicks;
        var prevTicks = Interlocked.Read(ref _lastTelemetryBroadcastTicks);
        if (nowTicks - prevTicks < minInterval.Ticks)
        {
            return false;
        }

        return Interlocked.CompareExchange(ref _lastTelemetryBroadcastTicks, nowTicks, prevTicks) == prevTicks;
    }

    public static (bool Allowed, string? ErrorMessage) ValidateAndTrackSerialSession(
        string meterSerial,
        string? remoteIp,
        int tagCount)
    {
        if (string.IsNullOrWhiteSpace(meterSerial))
            return (true, null);

        var key = meterSerial.Trim();
        var ip = string.IsNullOrWhiteSpace(remoteIp) ? "Unknown" : remoteIp.Trim();
        var now = DateTimeOffset.UtcNow;

        var session = _serialSessions.GetOrAdd(key, _ => new SerialSessionState());
        lock (session.SyncRoot)
        {
            // 1. Multi-IP Spoofing / Clone Detection:
            // If another IP is actively pushing this Serial within SessionIpLockDuration (3 minutes), block the rogue IP.
            if (!string.IsNullOrEmpty(session.PrimaryIp) &&
                !string.Equals(session.PrimaryIp, ip, StringComparison.OrdinalIgnoreCase) &&
                (now - session.LastAcceptedAt) < SessionIpLockDuration)
            {
                RecordSerialConflict(key, session.PrimaryIp, ip, "MULTI_IP_SPOOFING", now);
                return (false, $"Phát hiện xung đột bảo mật: Số chế tạo '{key}' đang hoạt động từ IP '{session.PrimaryIp}'. Từ chối gói tin trùng lặp/giả mạo từ IP '{ip}'.");
            }

            // 2. Anti-Flooding / DoS Sabotage Rate Limit per Serial:
            if ((now - session.WindowStart) > RateLimitWindow)
            {
                session.WindowStart = now;
                session.WindowHitCount = 1;
            }
            else
            {
                session.WindowHitCount++;
                if (session.WindowHitCount > MaxRequestsPerSerialWindow)
                {
                    RecordSerialConflict(key, session.PrimaryIp, ip, "RATE_LIMIT_FLOOD", now);
                    return (false, $"Phát hiện tần suất gửi bất thường ({session.WindowHitCount} gói/10s) cho Số chế tạo '{key}' từ IP '{ip}'. Đã chặn gói tin để chống phá hoại.");
                }
            }

            // 3. Same-IP Duplicate Client Collision Suppression:
            // If two REST Clients on the same IP (e.g. M130 & M131) push identical full frames (>= 20 tags) within 1.5s, suppress the duplicate.
            if (tagCount >= 20 &&
                session.LastFullFrameTagCount == tagCount &&
                (now - session.LastFullFrameAt).TotalMilliseconds < DuplicateFullFrameSuppressMs)
            {
                RecordSerialConflict(key, ip, ip, "DUPLICATE_CLIENT_SAME_IP", now);
                return (false, $"Phát hiện nhiều REST Client cùng gửi đồng thời một Số chế tạo '{key}' trên IP '{ip}'. Đã bỏ qua bản tin trùng lặp.");
            }

            session.PrimaryIp = ip;
            session.LastAcceptedAt = now;
            if (tagCount >= 20)
            {
                session.LastFullFrameAt = now;
                session.LastFullFrameTagCount = tagCount;
            }

            return (true, null);
        }
    }

    private static void RecordSerialConflict(string serial, string primaryIp, string offendingIp, string reason, DateTimeOffset now)
    {
        lock (_lock)
        {
            if (_serialConflicts.TryGetValue(serial, out var existing))
            {
                _serialConflicts[serial] = existing with
                {
                    PrimaryIp = primaryIp,
                    OffendingIp = offendingIp,
                    Reason = reason,
                    LastDetectedAt = now,
                    ConflictCount = existing.ConflictCount + 1
                };
                return;
            }

            if (_serialConflicts.Count >= MaxConflictEntries)
            {
                var oldestKey = _serialConflicts
                    .OrderBy(kvp => kvp.Value.LastDetectedAt)
                    .Select(kvp => kvp.Key)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(oldestKey))
                {
                    _serialConflicts.Remove(oldestKey);
                }
            }

            _serialConflicts[serial] = new SerialConflictEntry(serial, primaryIp, offendingIp, reason, now, 1);
        }
    }

    public static void ResetSerialSession(string? meterSerial)
    {
        if (string.IsNullOrWhiteSpace(meterSerial)) return;
        var key = meterSerial.Trim();
        _serialSessions.TryRemove(key, out _);
        lock (_lock)
        {
            _serialConflicts.Remove(key);
        }
    }

    public static void ResetAllSerialSessions()
    {
        _serialSessions.Clear();
        lock (_lock)
        {
            _serialConflicts.Clear();
        }
    }

    public static void RecordUnmatchedSerial(string? meterSerial, string? ip)
    {
        if (string.IsNullOrWhiteSpace(meterSerial)) return;
        var key = meterSerial.Trim();
        var now = DateTimeOffset.UtcNow;
        var remoteIp = string.IsNullOrWhiteSpace(ip) ? "Unknown" : ip;

        lock (_lock)
        {
            if (_unmatchedSerials.TryGetValue(key, out var existing))
            {
                _unmatchedSerials[key] = existing with
                {
                    RemoteIp = remoteIp,
                    LastSeenAt = now,
                    HitCount = existing.HitCount + 1
                };
                return;
            }

            if (_unmatchedSerials.Count >= MaxUnmatchedSerialEntries)
            {
                var oldestKey = _unmatchedSerials
                    .OrderBy(kvp => kvp.Value.LastSeenAt)
                    .Select(kvp => kvp.Key)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(oldestKey))
                {
                    _unmatchedSerials.Remove(oldestKey);
                }
            }

            _unmatchedSerials[key] = new UnmatchedSerialEntry(key, remoteIp, now, now, 1);
        }
    }

    public static IReadOnlyList<UnmatchedSerialEntry> GetRecentUnmatchedSerials()
    {
        lock (_lock)
        {
            return _unmatchedSerials.Values
                .OrderByDescending(x => x.LastSeenAt)
                .ToList();
        }
    }

    public static void RemoveUnmatchedSerial(string? meterSerial)
    {
        if (string.IsNullOrWhiteSpace(meterSerial)) return;
        lock (_lock)
        {
            _unmatchedSerials.Remove(meterSerial.Trim());
        }
    }

    public static bool IsInitialized => _isInitialized && (DateTime.UtcNow - _lastLoaded).TotalMinutes < 5;

    public static (string ApiKey, string AllowedIps, bool RequireApiKey, bool AllowLegacyTagRouting) GetCachedConfig()
    {
        lock (_lock)
        {
            return (_apiKey, _allowedIps, _requireApiKey, _allowLegacyTagRouting);
        }
    }

    public static void UpdateConfig(string apiKey, string allowedIps, bool requireApiKey, bool allowLegacyTagRouting = true)
    {
        lock (_lock)
        {
            _apiKey = apiKey ?? string.Empty;
            _allowedIps = allowedIps ?? string.Empty;
            _requireApiKey = requireApiKey;
            _allowLegacyTagRouting = allowLegacyTagRouting;
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

    public static string GenerateSecureKey(string prefix = "MDMS")
    {
        var bytes = new byte[24];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var base64 = Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
        return $"{prefix}_{base64}";
    }

    public static string HashSecret(string plainSecret)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainSecret.Trim()));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public static bool VerifySecretHash(string? providedSecret, string? expectedHash)
    {
        if (string.IsNullOrWhiteSpace(providedSecret) || string.IsNullOrWhiteSpace(expectedHash))
            return false;

        var computedHex = HashSecret(providedSecret);
        var computedBytes = Encoding.UTF8.GetBytes(computedHex);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedHash.Trim().ToLowerInvariant());

        return computedBytes.Length == expectedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(computedBytes, expectedBytes);
    }

    public static bool FixedTimeEquals(string? a, string? b)
    {
        if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
            return false;

        var bytesA = Encoding.UTF8.GetBytes(a.Trim());
        var bytesB = Encoding.UTF8.GetBytes(b.Trim());
        return bytesA.Length == bytesB.Length && CryptographicOperations.FixedTimeEquals(bytesA, bytesB);
    }

    public static string ExtractKeyPrefix(string plainSecret)
    {
        if (string.IsNullOrWhiteSpace(plainSecret)) return string.Empty;
        var trimmed = plainSecret.Trim();
        return trimmed.Length <= 12 ? trimmed : $"{trimmed[..12]}...";
    }

    public static string EncryptPointSecret(string? plainSecret, string? secretKey = null)
        => EmailSecretCryptoHelper.Encrypt(plainSecret, secretKey);

    public static string DecryptPointSecret(string? encryptedSecret, string? secretKey = null)
        => EmailSecretCryptoHelper.Decrypt(encryptedSecret, secretKey);
}
