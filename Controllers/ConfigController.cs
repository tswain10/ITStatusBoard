using Microsoft.AspNetCore.Mvc;

// Add route prefix for controller
[Route("[controller]")]
public class ConfigController : Controller
{
    private readonly ApiStatusService _service;

    public ConfigController(ApiStatusService service)
    {
        _service = service;
    }

    // GET /Config
    [HttpGet("")]
    public IActionResult Index()
    {
        return View(_service.GetConfigs());
    }

    // POST /Config/Add
    [HttpPost("Add")]
    [ValidateAntiForgeryToken]
    public IActionResult Add(ApiCheckConfig config)
    {
        if (ModelState.IsValid)
        {
            _service.AddConfig(config);
            return RedirectToAction("Index");
        }
        return View("Index", _service.GetConfigs());
    }

    // POST /Config/Remove
    [HttpPost("Remove")]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(string url)
    {
        _service.RemoveConfig(url);
        return RedirectToAction("Index");
    }

    // POST /Config/Edit
    [HttpPost("Edit")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(string OriginalUrl, string Name, string Url, string Type)
    {
        var config = _service.GetConfigs().FirstOrDefault(c => c.Url == OriginalUrl);
        if (config != null)
        {
            config.Name = Name;
            config.Url = Url;
            config.Type = Type;
            // Remove and re-add if URL changed
            if (OriginalUrl != Url)
            {
                _service.RemoveConfig(OriginalUrl);
                _service.AddConfig(config);
            }
            else
            {
                // Just save configs
                var configs = _service.GetConfigs();
                var json = System.Text.Json.JsonSerializer.Serialize(configs, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "ApiConfigs.json"), json);
            }
        }
        return RedirectToAction("Index");
    }
}
