using Microsoft.AspNetCore.Mvc;
using MyAspNetCoreApp.Models;
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
            return View(apiStatuses);
        }
    }
}