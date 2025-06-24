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
}
