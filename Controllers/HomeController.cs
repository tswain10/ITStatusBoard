using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MyAspNetCoreApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiStatusService _service;
        public HomeController(ApiStatusService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var apiStatuses = await _service.CheckAllAsync(); // Check all statuses on each load
            var statsList = _service.GetAllStats();
            var summaries = _service.GetTypeSummaries();
            ViewBag.Summaries = summaries;
            ViewBag.Websites = apiStatuses.Where(x => x.Type == "Website").ToList();
            ViewBag.Apis = apiStatuses.Where(x => x.Type == "API").ToList();
            ViewBag.Servers = apiStatuses.Where(x => x.Type == "Server").ToList();
            // For per-entity stats in tables
            var statsByUrl = statsList.ToDictionary(s => s.Url, s => (s.UptimePercent, s.DowntimePercent, s.AvgResponseTimeMs));
            ViewBag.StatsByUrl = statsByUrl;
            return View();
        }

        public IActionResult EntityDetails(string url)
        {
            if (string.IsNullOrEmpty(url)) return RedirectToAction("Index");
            var config = _service.GetConfigs().FirstOrDefault(c => c.Url == url);
            if (config == null) return NotFound();
            var history = _service.GetHistory(url).OrderByDescending(h => h.Timestamp).Take(10).ToList();
            var stats = _service.GetStats(url);
            ViewBag.Config = config;
            ViewBag.Stats = stats;
            return View(history);
        }
    }
}