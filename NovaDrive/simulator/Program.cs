using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Grpc.Net.Client;
using NovaDrive.Api.Grpc;

const bool UseGrpc = true; // flip to false to fall back to REST

// Configuration
var apiBaseUrl  = Environment.GetEnvironmentVariable("API_BASE_URL")  ?? "http://localhost:8080";
var grpcBaseUrl = Environment.GetEnvironmentVariable("GRPC_BASE_URL") ?? "http://localhost:8081";
const int    TelemetryIntervalMs   = 3_000;
const int    DiagnosticIntervalMs  = 15_000;
const double DiagnosticFaultChance = 0.10;

var vehicles = new[]
{
    (VehicleId: Guid.Parse("22b21e43-0151-4ef4-8ea6-1e37d89d5607"),
     ApiKey:    "KTMoiTpEf0as0sf9SLJY0Fc+rbZI5iN/pJf+efIa93I="),
};

// Validation

if (vehicles[0].ApiKey == "PASTE-PLAIN-TEXT-KEY-HERE")
{
    Console.WriteLine(
        "ERROR: No vehicles configured.\n" +
        "Edit the vehicles list at the top of Program.cs and paste in your\n" +
        "vehicle ID and plain-text API key before running.");
    return;
}

// JSON

var json = new JsonSerializerOptions
{
    PropertyNamingPolicy   = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters             = { new JsonStringEnumConverter() },
};

// Wait for API

Console.WriteLine($"Waiting for API at {apiBaseUrl}...");
using var healthClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

var deadline = DateTime.UtcNow.AddSeconds(30);
while (DateTime.UtcNow < deadline)
{
    try { if ((await healthClient.GetAsync("/health")).IsSuccessStatusCode) break; }
    catch { }
    Console.WriteLine("  Not ready — retrying in 2s...");
    await Task.Delay(2000);
}
Console.WriteLine("API is healthy — starting simulation.\n");

// GPS route (circle around Ghent)

static (double Lat, double Lon)[] BuildRoute(double cLat, double cLon, double km, int steps)
{
    var route = new (double, double)[steps];
    for (var i = 0; i < steps; i++)
    {
        var a    = 2 * Math.PI * i / steps;
        var lat  = cLat + (km / 111.0) * Math.Sin(a);
        var lon  = cLon + (km / (111.0 * Math.Cos(cLat * Math.PI / 180.0))) * Math.Cos(a);
        route[i] = (Math.Round(lat, 6), Math.Round(lon, 6));
    }
    return route;
}

var route = BuildRoute(cLat: 51.0543, cLon: 3.7174, km: 1.5, steps: 120);

// Sensor fault data

var sensorErrors = new Dictionary<string, string[]>
{
    ["Lidar"]  = ["LIDAR_RETURN_LOSS", "LIDAR_BEAM_SCATTER", "LIDAR_TEMP_HIGH",  "LIDAR_CALIBRATION_DRIFT"],
    ["Radar"]  = ["RADAR_SIGNAL_WEAK", "RADAR_MULTIPATH",    "RADAR_FREQ_DRIFT", "RADAR_BLOCKAGE_DETECTED"],
    ["Camera"] = ["CAM_BLUR_DETECTED", "CAM_LENS_DIRTY",     "CAM_EXPOSURE_ERR", "CAM_FRAME_DROP"],
};

// Severity weighted: Info 40%, Warning 35%, Error 20%, Critical 5%
string RandomSeverity()
{
    var roll = Random.Shared.Next(100);
    return roll < 40 ? "Info" : roll < 75 ? "Warning" : roll < 95 ? "Error" : "Critical";
}

// Shared diagnostic sender (used by both REST and gRPC vehicle runners)

async Task SendDiagnostic(Guid vehicleId, string apiKey, double speed, double lat, double lon,
    string tag, CancellationToken ct)
{
    using var client = new HttpClient(new ApiKeyHandler(vehicleId, apiKey))
    {
        BaseAddress = new Uri(apiBaseUrl)
    };

    var sensors   = sensorErrors.Keys.ToArray();
    var sensor    = sensors[Random.Shared.Next(sensors.Length)];
    var errors    = sensorErrors[sensor];
    var errorCode = errors[Random.Shared.Next(errors.Length)];
    var severity  = RandomSeverity();

    var rawData = JsonSerializer.Serialize(new
    {
        sensorId     = $"{sensor.ToUpperInvariant()}_01",
        errorCode,
        value        = Math.Round(Random.Shared.NextDouble() * 100, 2),
        threshold    = Math.Round(60 + Random.Shared.NextDouble() * 30, 2),
        timestamp    = DateTime.UtcNow.ToString("o"),
        vehicleSpeed = Math.Round(speed, 1),
        location     = new { lat, lon },
    });

    var diagnostic = new
    {
        vehicleId,
        sensorType    = sensor,
        errorCode,
        severity,
        rawSensorData = rawData,
    };

    try
    {
        var r = await client.PostAsJsonAsync("/api/v1/diagnostics", diagnostic, json, ct);
        if (r.IsSuccessStatusCode)
            Log(tag, ConsoleColor.Yellow,
                $"Diagnostic  sensor={sensor,-6} code={errorCode,-30} severity={severity}");
        else
            Log(tag, ConsoleColor.Red, $"Diagnostic rejected — HTTP {(int)r.StatusCode}");
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        Log(tag, ConsoleColor.Red, $"Diagnostic error — {ex.Message}");
    }
}

// REST vehicle runner

async Task RunVehicle(Guid vehicleId, string apiKey, CancellationToken ct)
{
    var tag = vehicleId.ToString()[..8];

    using var client = new HttpClient(new ApiKeyHandler(vehicleId, apiKey))
    {
        BaseAddress = new Uri(apiBaseUrl)
    };

    var routeIndex       = Random.Shared.Next(route.Length);
    var battery          = 100;
    var speed            = 50.0;
    var temperature      = 25.0;
    var lastDiagnosticAt = DateTime.UtcNow;

    Log(tag, ConsoleColor.Green, $"Started (REST) — vehicle {vehicleId}");

    while (!ct.IsCancellationRequested)
    {
        routeIndex  = (routeIndex + 1) % route.Length;
        speed       = Math.Clamp(speed + Random.Shared.NextDouble() * 10 - 5, 30, 80);
        temperature = Math.Clamp(temperature + (Random.Shared.NextDouble() - 0.5), 20, 45);
        battery     = battery <= 10 ? 95 : Math.Max(0, battery - Random.Shared.Next(0, 2));

        var (lat, lon) = route[routeIndex];

        var telemetry = new
        {
            latitude            = lat,
            longitude           = lon,
            speedKmh            = Math.Round(speed, 1),
            batteryPercentage   = battery,
            hardwareTemperature = Math.Round(temperature, 1),
        };

        try
        {
            var r = await client.PostAsJsonAsync("/api/v1/telemetry", telemetry, json, ct);
            if (r.IsSuccessStatusCode)
                Log(tag, ConsoleColor.Cyan,
                    $"Telemetry  lat={lat:F4} lon={lon:F4}  speed={speed:F1} km/h  battery={battery}%");
            else
            {
                var body = await r.Content.ReadAsStringAsync(ct);
                Log(tag, ConsoleColor.Yellow, $"Telemetry rejected — HTTP {(int)r.StatusCode} — {body}");
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log(tag, ConsoleColor.Red, $"Telemetry error — {ex.Message}");
        }

        if ((DateTime.UtcNow - lastDiagnosticAt).TotalMilliseconds >= DiagnosticIntervalMs)
        {
            lastDiagnosticAt = DateTime.UtcNow;
            if (Random.Shared.NextDouble() <= DiagnosticFaultChance)
                await SendDiagnostic(vehicleId, apiKey, speed, lat, lon, tag, ct);
            else
                Log(tag, ConsoleColor.DarkGray, "Sensor check OK");
        }

        await Task.Delay(TelemetryIntervalMs, ct);
    }
}

// gRPC vehicle runner

async Task RunVehicleGrpc(Guid vehicleId, string apiKey, CancellationToken ct)
{
    var tag = vehicleId.ToString()[..8];

    using var channel = GrpcChannel.ForAddress(grpcBaseUrl, new GrpcChannelOptions
    {
        HttpHandler = new HttpClientHandler()
    });
    var grpcClient = new TelemetryIngest.TelemetryIngestClient(channel);

    using var call = grpcClient.StreamTelemetry(cancellationToken: ct);

    var routeIndex       = Random.Shared.Next(route.Length);
    var battery          = 100;
    var speed            = 50.0;
    var temperature      = 25.0;
    var lastDiagnosticAt = DateTime.UtcNow;

    Log(tag, ConsoleColor.Green, $"Started (gRPC) — vehicle {vehicleId}");

    while (!ct.IsCancellationRequested)
    {
        routeIndex  = (routeIndex + 1) % route.Length;
        speed       = Math.Clamp(speed + Random.Shared.NextDouble() * 10 - 5, 30, 80);
        temperature = Math.Clamp(temperature + (Random.Shared.NextDouble() - 0.5), 20, 45);
        battery     = battery <= 10 ? 95 : Math.Max(0, battery - Random.Shared.Next(0, 2));

        var (lat, lon) = route[routeIndex];

        try
        {
            await call.RequestStream.WriteAsync(new TelemetryFrame
            {
                VehicleId           = vehicleId.ToString(),
                ApiKey              = apiKey,
                Latitude            = lat,
                Longitude           = lon,
                SpeedKmh            = Math.Round(speed, 1),
                BatteryPercentage   = battery,
                HardwareTemperature = Math.Round(temperature, 1),
            }, ct);

            if (await call.ResponseStream.MoveNext(ct))
            {
                var ack = call.ResponseStream.Current;
                if (ack.Accepted)
                    Log(tag, ConsoleColor.Cyan,
                        $"gRPC ack  lat={lat:F4} lon={lon:F4}  speed={speed:F1} km/h  battery={battery}%");
                else
                    Log(tag, ConsoleColor.Yellow, $"gRPC rejected — {ack.Message}");
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log(tag, ConsoleColor.Red, $"gRPC error — {ex.Message}");
        }

        // Diagnostics always go over REST — infrequent, no need for gRPC here
        if ((DateTime.UtcNow - lastDiagnosticAt).TotalMilliseconds >= DiagnosticIntervalMs)
        {
            lastDiagnosticAt = DateTime.UtcNow;
            if (Random.Shared.NextDouble() <= DiagnosticFaultChance)
                await SendDiagnostic(vehicleId, apiKey, speed, lat, lon, tag, ct);
            else
                Log(tag, ConsoleColor.DarkGray, "Sensor check OK");
        }

        await Task.Delay(TelemetryIntervalMs, ct);
    }

    await call.RequestStream.CompleteAsync();
}

// Start all vehicles concurrently

using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\nStopping...");
    cts.Cancel();
};

var runner = UseGrpc
    ? (Func<Guid, string, CancellationToken, Task>)RunVehicleGrpc
    : RunVehicle;

try
{
    await Task.WhenAll(vehicles.Select(v => runner(v.VehicleId, v.ApiKey, cts.Token)));
}
catch (OperationCanceledException) { }

Console.WriteLine("Simulator stopped.");

// Helpers

static void Log(string tag, ConsoleColor color, string message)
{
    Console.ForegroundColor = color;
    Console.Write($"{DateTime.Now:HH:mm:ss}  [{tag}]  ");
    Console.ResetColor();
    Console.WriteLine(message);
}

sealed class ApiKeyHandler(Guid vehicleId, string apiKey) : DelegatingHandler(new HttpClientHandler())
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
    {
        req.Headers.TryAddWithoutValidation("X-Api-Key",    apiKey);
        req.Headers.TryAddWithoutValidation("X-Vehicle-Id", vehicleId.ToString());
        return base.SendAsync(req, ct);
    }
}