
using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sutnance.Data;
using sutnance.Models;

namespace sutnance.Controllers
{

    [Route("[controller]")]
    public class EtatController(MachineManager machineManager, sutnanceContext ctx) : Controller
    {
        [HttpGet]
        /*[Route("{page:int?}")]*/
        public async Task<IActionResult> Index(string? search, MachineState? state, int page = 0)
        {
            ViewBag.machines = await machineManager.Get(search, state, page);
            ViewBag.machineTypes = await ctx.MachineTypes.AsNoTracking().ToListAsync();
            return View();
        }
        [Route("Create")]
        [HttpPost]
        public async Task<IActionResult> Create(string name, int type, string ip, string site, string manager, string docs)
        {
            await machineManager.Create(name, type, ip, site, manager, docs);
            return RedirectToAction(nameof(Index));
        }
        [Route("update")]
        [HttpPost]
        public async Task<IActionResult> Update(string id, string name, int type, string ip, string site, string manager, string docs)
        {
            await machineManager.Update(id, name, type, ip, site, manager, docs);
            return RedirectToAction(nameof(Details), new { id = id });
        }
        //TODO ADD UPDATE DIPSHIT
        [Route("{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var machine = await machineManager.Get(id);
            if (machine is null)
            {
                return NotFound();
            }
            ViewBag.machineTypes = await ctx.MachineTypes.AsNoTracking().ToListAsync();
            return View(machine);
        }
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await machineManager.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
