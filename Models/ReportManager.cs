using Microsoft.EntityFrameworkCore;
using sutnance.Data;
namespace sutnance.Models;

public class ReportManager(sutnanceContext dbContext)
{
    private int? _lastCachedReportId = null;
    private async Task<string> GetReportId()
    {
        return Guid.NewGuid().ToString();
        int lastId;
        if (_lastCachedReportId == null)
        {
        var lastReport = await dbContext.Reports
            .OrderByDescending(r => r.CreationTime)
            .FirstOrDefaultAsync();

        if (lastReport == null)
            _lastCachedReportId = 1;
            return "R-001";

        lastId = int.Parse(lastReport.Id.Split('-')[1]) + 1;

        }
        else
        {
            lastId = (int)_lastCachedReportId + 1 ;
        }
        _lastCachedReportId = lastId;
        return $"R-{lastId + 1:000}";
    }

    /*public async Task<List<Machine>> Get(string? search, MachineState? state, int page = 0)
    {
        const int pageSize = 10;
        int skipCount = pageSize * page;

        var query =
            from machine in dbContext.Machines.AsQueryable().OrderByDescending(m => m.CreationTime)
            join report in dbContext.Reports.AsQueryable()
                on machine.Id equals report.MachineId
                into reportjoin
            select new Machine()
            {
                Id = machine.Id,
                Name = machine.Name,
                Type = machine.Type,
                Ip = machine.Ip,
                Site = machine.Site,
                BootTime = machine.BootTime,

                LastReport = reportjoin
                    .OrderByDescending(r => r.CreationTime)
                    .FirstOrDefault()
            };
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e =>

                e.Name.ToLower().Contains(search) ||
                e.Ip.ToLower().Contains(search)
            );
        }

        if (state != null)
        {
            query = query.Where(e => (e.LastReport != null) && e.LastReport.State == state);
        }

        return await query.Skip(skipCount).Take(pageSize).ToListAsync();
    }
    */
    public async Task<Report> makeReport(string machineId, MachineState state)
    {

        return new Report()
        {
            Id = await GetReportId(),
            State = state,
            MachineId = machineId,
        };
    }
    public async Task Create(string machineId, MachineState state)
    {

        var report = await this.makeReport(machineId, state);
        dbContext.Reports.Add(report);
        await dbContext.SaveChangesAsync();
    }

    /*public async Task Update(string id, string name, string type, string ip, string site, DateTime bootTime)
    {
        var machine = new Machine()
        {
            Id = id,
            Name = name,
            Type = type,
            Ip = ip,
            Site = site,
            BootTime = bootTime,
        };
        dbContext.Machines.Update(machine);
        await dbContext.SaveChangesAsync();
    }*/

    /*
    public async Task Delete(string id)
    {
        var machine = await dbContext.Machines.FindAsync(id) ?? throw new InvalidOperationException();
        dbContext.Machines.Remove(machine);
        await dbContext.SaveChangesAsync();
    }
    */

    public async Task SeedAsync()
    {
        if (await dbContext.Reports.AnyAsync())
            return;

        var machineIds = await dbContext.Machines.Select(m => m.Id).ToListAsync();
        foreach (var machineid in machineIds)
        {
            var rnd = new Random();

            var states = Enum.GetValues<MachineState>();
            var randomState = states[rnd.Next(states.Length)];
            await this.Create(machineid, randomState);
        }


        await dbContext.SaveChangesAsync();
    }
}
