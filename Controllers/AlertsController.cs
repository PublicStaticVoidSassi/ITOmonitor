using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sutnance.Data;
using sutnance.hacking;

namespace sutnance.Controllers
{
    [Route("[controller]")]
    public class AlertsController(
        // sutnanceContext context,
        IAlertsManager alertsManager) : Controller
    {
        public IActionResult Index()
        {
            // ViewBag.alerts = alertsManager.GetAlerts();
            return View(alertsManager.GetAlerts());
        }
        [Route("dismiss/{id}")]
        public IActionResult Dismiss(int id){
            alertsManager.Dismiss(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
