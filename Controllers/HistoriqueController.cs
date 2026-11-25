
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sutnance.hacking;
using sutnance.Models;

namespace sutnance.Controllers
{
    [Route("[controller]")]
    public class HistoriqueController(IHistoriqueManager historiqueManager) : Controller
    {
        //TODO MAKE IT A VIEW DIPSHIT
        // [Route("{id?}")]
        public async Task<IActionResult> Index(string? search = null, int page = 0)
        {
            return View(
                await historiqueManager.Get(search, page)
            );
        }
        [HttpPost]
        [Route("addnote")]
        public async Task<IActionResult> AddNote(string? id, string? note)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Historique ID is missing.");

            if (string.IsNullOrWhiteSpace(note))
                return BadRequest("Note content is missing.");

            try
            {
                historiqueManager.AddNote(id, note);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
