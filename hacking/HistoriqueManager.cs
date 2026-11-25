using Microsoft.EntityFrameworkCore;
using sutnance.Data;
using sutnance.Models;

namespace sutnance.hacking;

public interface IHistoriqueManager
{
    Task<List<Historique>> Get(string? id = null, int page = 0, int count = 10);
    void AddNote(string id, string text);
}

public class HistoriqueManager(sutnanceContext ctx) : IHistoriqueManager
{
    public void AddNote(string id, string text) {
        Note note = new Note();
        note.HistoriqueId = id;
        note.Content = text;

        ctx.Notes.Add(note);
        ctx.SaveChanges();
    }

    public async Task<List<Historique>> Get(string? id=null, int page = 0, int count = 10)
    {
        Console.WriteLine($"ID SEND IS {id}");
        int skipCount = page * count;
        var q = ctx.Historiques.AsNoTracking();
        if (!string.IsNullOrEmpty(id)){
            q = q.Where(h => h.MachineId.Equals(id));
        }
        q = q.OrderByDescending(h=>h.From).Skip(skipCount).Take(count); //.Where(h => h.MachineId.Equals(id));
        var rawHistory = await q.ToListAsync();
        foreach (var rh in rawHistory)
        {
            rh.Note = ctx.Notes.OrderByDescending(n=>n.CreationTime).FirstOrDefault(n => n.HistoriqueId == rh.Id);
        }
        return rawHistory;
    }
}
