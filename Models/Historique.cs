using Microsoft.EntityFrameworkCore;

namespace sutnance.Models;
// this is a view
public class Historique{
    public string Id { get; set; }
    public string MachineId { get; set; }
    public MachineState State { get; set; }
    public DateTime From { get; set; }
    public DateTime? Until { get; set; }
    public Note? Note { get; set; }
}
