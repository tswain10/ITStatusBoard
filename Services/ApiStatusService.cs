using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.Json;
using System.Diagnostics; // For Stopwatch

public class ApiStatusService
{
    private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiConfigs.json");
    private readonly string _historyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiStatusHistory.json");
    private readonly string _notifyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiNotifications.log");
    private readonly List<ApiCheckConfig> _configs;
    private readonly HttpClient _httpClient;
    private Dictionary<string, List<ApiStatusHistoryEntry>> _historyCache = new();

    public ApiStatusService()
    {
        _configs = LoadConfigs();
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10) 
        };
        LoadHistory();
    }

    private List<ApiCheckConfig> LoadConfigs()
    {
        if (File.Exists(_configPath))
        {
            try
            {
                var json = File.ReadAllText(_configPath);
                var configs = JsonSerializer.Deserialize<List<ApiCheckConfig>>(json);
                return configs ?? new List<ApiCheckConfig>();
            }
            catch
            {
                return new List<ApiCheckConfig>();
            }
        }
        return new List<ApiCheckConfig>();
    }

    private void SaveConfigs()
    {
        var json = JsonSerializer.Serialize(_configs, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configPath, json);
    }

    public List<ApiCheckConfig> GetConfigs() => _configs;

    public void AddConfig(ApiCheckConfig config)
    {
        if (!_configs.Exists(c => c.Url == config.Url))
        {
            _configs.Add(config);
            SaveConfigs();
        }
    }

    public void RemoveConfig(string url)
    {
        var item = _configs.Find(c => c.Url == url);
        if (item != null)
        {
            _configs.Remove(item);
            SaveConfigs();
        }
    }

    // Response time and history tracking
    public async Task<List<ApiStatus>> CheckAllAsync()
    {
        var results = new List<ApiStatus>();
        foreach (var config in _configs)
        {
            var status = new ApiStatus
            {
                ApiName = config.Name,
                LastChecked = DateTime.Now,
                Status = "Offline",
                Url = config.Url,
                Type = config.Type
            };

            // Resolve server IP
            try
            {
                var uri = new Uri(config.Url);
                var host = uri.Host;
                var ips = await Dns.GetHostAddressesAsync(host);
                status.ServerIp = ips.Length > 0 ? ips[0].ToString() : "N/A";
            }
            catch
            {
                status.ServerIp = "N/A";
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var response = await _httpClient.GetAsync(config.Url);
                stopwatch.Stop();
                status.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                status.Status = response.IsSuccessStatusCode ? "Online" : "Offline";
                if (!response.IsSuccessStatusCode)
                    status.ErrorMessage = response.ReasonPhrase;
            }
            catch (TaskCanceledException)
            {
                stopwatch.Stop();
                status.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                status.ErrorMessage = "Timeout";
                status.Status = "Offline";
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                status.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                status.ErrorMessage = ex.Message;
            }

            results.Add(status);

            // --- Historical tracking ---
            AddHistoryEntry(config.Url, status);

            // --- Notification system (basic) ---
            CheckAndNotify(config.Url, status.Status, status.LastChecked);
        }
        SaveHistory();
        return results;
    }

    // --- Historical Uptime/Outage Tracking ---
    private void LoadHistory()
    {
        if (File.Exists(_historyPath))
        {
            try
            {
                var json = File.ReadAllText(_historyPath);
                _historyCache = JsonSerializer.Deserialize<Dictionary<string, List<ApiStatusHistoryEntry>>>(json)
                    ?? new Dictionary<string, List<ApiStatusHistoryEntry>>();
            }
            catch
            {
                _historyCache = new Dictionary<string, List<ApiStatusHistoryEntry>>();
            }
        }
        else
        {
            _historyCache = new Dictionary<string, List<ApiStatusHistoryEntry>>();
        }
    }

    private void SaveHistory()
    {
        var json = JsonSerializer.Serialize(_historyCache, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_historyPath, json);
    }

    private void AddHistoryEntry(string url, ApiStatus status)
    {
        if (!_historyCache.ContainsKey(url))
            _historyCache[url] = new List<ApiStatusHistoryEntry>();
        _historyCache[url].Add(new ApiStatusHistoryEntry
        {
            Timestamp = status.LastChecked,
            Status = status.Status,
            ResponseTimeMs = status.ResponseTimeMs
        });
        // Keep only last 100 entries per service
        if (_historyCache[url].Count > 100)
            _historyCache[url].RemoveAt(0);
    }

    public List<ApiStatusHistoryEntry> GetHistory(string url)
    {
        return _historyCache.ContainsKey(url) ? _historyCache[url] : new List<ApiStatusHistoryEntry>();
    }

    // --- Notification System (basic file log) ---
    private Dictionary<string, string> _lastStatus = new();
    private void CheckAndNotify(string url, string newStatus, DateTime when)
    {
        if (!_lastStatus.TryGetValue(url, out var last))
            last = "";
        if (last != newStatus)
        {
            var msg = $"{when:u} [{url}] status changed: {last} -> {newStatus}{Environment.NewLine}";
            File.AppendAllText(_notifyPath, msg);
            _lastStatus[url] = newStatus;
        }
    }

    // --- Statistics Methods ---
    public (double UptimePercent, double DowntimePercent, double AvgResponseTimeMs) GetStats(string url)
    {
        var history = GetHistory(url);
        if (history.Count == 0) return (0, 0, 0);
        int up = history.Count(h => h.Status == "Online");
        int down = history.Count(h => h.Status != "Online");
        double uptime = 100.0 * up / history.Count;
        double downtime = 100.0 * down / history.Count;
        double avgResp = history.Count > 0 ? history.Average(h => h.ResponseTimeMs) : 0;
        return (uptime, downtime, avgResp);
    }

    public List<(string Name, string Url, string Type, double UptimePercent, double DowntimePercent, double AvgResponseTimeMs)> GetAllStats()
    {
        var configs = GetConfigs();
        var result = new List<(string, string, string, double, double, double)>();
        foreach (var cfg in configs)
        {
            var stats = GetStats(cfg.Url);
            result.Add((cfg.Name, cfg.Url, cfg.Type, stats.UptimePercent, stats.DowntimePercent, stats.AvgResponseTimeMs));
        }
        return result;
    }

    // --- Summary Statistics for Dashboard Cards ---
    public List<(string Type, int Total, int Online, int Offline, double AvgUptime, double AvgDowntime, double AvgResponseTimeMs)> GetTypeSummaries()
    {
        var configs = GetConfigs();
        var types = new[] { "Website", "API", "Server" };
        var result = new List<(string, int, int, int, double, double, double)>();
        foreach (var type in types)
        {
            var items = configs.Where(c => c.Type == type).ToList();
            int total = items.Count;
            int online = 0, offline = 0;
            double sumUptime = 0, sumDowntime = 0, sumResp = 0;
            foreach (var cfg in items)
            {
                var stats = GetStats(cfg.Url);
                // Use last known status for online/offline
                var history = GetHistory(cfg.Url);
                if (history.Count > 0 && history.Last().Status == "Online") online++;
                else offline++;
                sumUptime += stats.UptimePercent;
                sumDowntime += stats.DowntimePercent;
                sumResp += stats.AvgResponseTimeMs;
            }
            double avgUptime = total > 0 ? sumUptime / total : 0;
            double avgDowntime = total > 0 ? sumDowntime / total : 0;
            double avgResp = total > 0 ? sumResp / total : 0;
            result.Add((type, total, online, offline, avgUptime, avgDowntime, avgResp));
        }
        return result;
    }
}

// Extend ApiStatus to include response time
public partial class ApiStatus
{
    public long ResponseTimeMs { get; set; }
}
