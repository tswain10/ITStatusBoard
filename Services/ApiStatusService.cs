using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.Json;

public class ApiStatusService
{
    private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiConfigs.json");
    private readonly List<ApiCheckConfig> _configs;
    private readonly HttpClient _httpClient = new HttpClient();

    public ApiStatusService()
    {
        _configs = LoadConfigs();
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

    public async Task<List<ApiStatus>> CheckAllAsync()
    {
        var results = new List<ApiStatus>();
        foreach (var config in _configs)
        {
            var status = new ApiStatus
            {
                ApiName = config.Name,
                LastChecked = DateTime.Now,
                Status = "Offline"
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

            try
            {
                var response = await _httpClient.GetAsync(config.Url);
                status.Status = response.IsSuccessStatusCode ? "Online" : "Offline";
                if (!response.IsSuccessStatusCode)
                    status.ErrorMessage = response.ReasonPhrase;
            }
            catch (Exception ex)
            {
                status.ErrorMessage = ex.Message;
            }
            status.ApiName = config.Name;
            status.ServerIp = status.ServerIp;
            status.LastChecked = DateTime.Now;
            status.ErrorMessage = status.ErrorMessage;
            status.Status = status.Status;
            status.ApiName = config.Name;
            status.ServerIp = status.ServerIp;
            status.LastChecked = DateTime.Now;
            status.ErrorMessage = status.ErrorMessage;
            status.Status = status.Status;
            status.ApiName = config.Name;
            status.ServerIp = status.ServerIp;
            status.LastChecked = DateTime.Now;
            status.ErrorMessage = status.ErrorMessage;
            status.Status = status.Status;
            status.ApiName = config.Name;
            results.Add(status);
        }
        return results;
    }
}
