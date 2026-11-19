using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MDMS.HES.Core.Interfaces;
using MDMS.HES.Core.Models;
using Microsoft.Extensions.Logging;

namespace MDMS.HES.Infrastructure.Uplink;

public class RestTelemetryPublisher : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ICircuitBreaker _circuitBreaker;
    private readonly ILogger<RestTelemetryPublisher>? _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public string TargetUrl { get; set; } = "http://127.0.0.1:19009/IoT/telemetry";
    public string? GlobalApiKey { get; set; }

    public RestTelemetryPublisher(
        ICircuitBreaker circuitBreaker,
        string? targetUrl = null,
        string? globalApiKey = null,
        ILogger<RestTelemetryPublisher>? logger = null)
    {
        _circuitBreaker = circuitBreaker;
        _logger = logger;
        if (!string.IsNullOrWhiteSpace(targetUrl))
            TargetUrl = targetUrl;
        GlobalApiKey = globalApiKey;

        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            MaxConnectionsPerServer = 50,
            ConnectTimeout = TimeSpan.FromSeconds(3),
            EnableMultipleHttp2Connections = false
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(4)
        };
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<bool> PublishTelemetryAsync(
        TelemetryPayload payload,
        string serial,
        string? apiKey = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Guard with Circuit Breaker
        if (!_circuitBreaker.CanPublish())
        {
            return false;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, TargetUrl);

            // Add standard headers expected by MDMSv2 IoTController
            if (!string.IsNullOrWhiteSpace(serial))
            {
                request.Headers.TryAddWithoutValidation("X-Meter-Serial", serial);
                request.Headers.TryAddWithoutValidation("Serial", serial);
            }

            var effectiveKey = !string.IsNullOrWhiteSpace(apiKey) ? apiKey : GlobalApiKey;
            if (!string.IsNullOrWhiteSpace(effectiveKey))
            {
                request.Headers.TryAddWithoutValidation("X-IoT-Api-Key", effectiveKey);
                request.Headers.TryAddWithoutValidation("Private-key", effectiveKey);
                request.Headers.TryAddWithoutValidation("Global-key", effectiveKey);
            }

            string json = JsonSerializer.Serialize(payload, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                _circuitBreaker.RecordSuccess();
                return true;
            }

            // Non-success status code (e.g. 500, 502, 503)
            int statusCode = (int)response.StatusCode;
            string err = $"HTTP {statusCode} {response.ReasonPhrase}";
            if (statusCode >= 500)
            {
                _circuitBreaker.RecordFailure(err);
            }
            else
            {
                _logger?.LogWarning("[Publisher] Endpoint returned {Status} for serial {Serial}", err, serial);
            }
            return false;
        }
        catch (TaskCanceledException tex) when (!cancellationToken.IsCancellationRequested)
        {
            _circuitBreaker.RecordFailure($"Timeout ({tex.Message})");
            return false;
        }
        catch (HttpRequestException hex)
        {
            _circuitBreaker.RecordFailure($"Connection failed: {hex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            _circuitBreaker.RecordFailure($"Unexpected: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ProbeServerAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, TargetUrl);
            request.Headers.TryAddWithoutValidation("X-Probe", "true");
            if (!string.IsNullOrWhiteSpace(GlobalApiKey))
            {
                request.Headers.TryAddWithoutValidation("X-IoT-Api-Key", GlobalApiKey);
            }

            // Minimal probe payload
            var probePayload = new TelemetryPayload
            {
                Meter = "PROBE",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Values = new List<TelemetryValueItem>()
            };

            string json = JsonSerializer.Serialize(probePayload, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            using var response = await _httpClient.SendAsync(request, cts.Token);
            
            // Any HTTP response (including 400/401/404) means the server and network socket are reachable!
            if ((int)response.StatusCode < 500)
            {
                _circuitBreaker.RecordSuccess();
                return true;
            }

            _circuitBreaker.RecordFailure($"Probe returned {(int)response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            _circuitBreaker.RecordFailure($"Probe unreachable: {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
