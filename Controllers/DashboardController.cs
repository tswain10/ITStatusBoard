using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyAspNetCoreApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApiStatusService _service;

        public DashboardController(ApiStatusService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var apiStatuses = await _service.CheckAllAsync();
            var statsList = _service.GetAllStats();
            var statsByUrl = new Dictionary<string, (double, double, double)>();
            foreach (var s in statsList)
                statsByUrl[s.Url] = (s.UptimePercent, s.DowntimePercent, s.AvgResponseTimeMs);
            ViewBag.StatsByUrl = statsByUrl;
            return View(apiStatuses);
        }
    }
}