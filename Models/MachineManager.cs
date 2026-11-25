using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using sutnance.Data;
using sutnance.hacking;

namespace sutnance.Models;

public class MachineManager(sutnanceContext dbContext)
{
    private async Task<string> GetMachineId()
    {
        try
        {
            var lastMachine =
                await dbContext.Machines.OrderByDescending(m => m.CreationTime).Take(1).SingleAsync();
            var lastId = Convert.ToInt32(lastMachine.Id.Split('-')[1]);
            return "EQ-" + ((lastId + 1).ToString());

        }
        catch (InvalidOperationException)
        {
            return "EQ-001";
        }
    }

    public async Task<Machine?> Get(string id)
    {
        var query =
            from machine in dbContext.Machines
            where machine.Id == id

            join report in dbContext.Reports
                on machine.Id equals report.MachineId
                into reportJoin

            select new Machine
            {
                Id = machine.Id,
                Name = machine.Name,
                Type = machine.Type,
                Ip = machine.Ip,
                Site = machine.Site,
                CreationTime = machine.CreationTime,
                TypeId = machine.TypeId,
                docs = machine.docs,
                Manager = machine.Manager,

                LastReport = reportJoin
                    .OrderByDescending(r => r.CreationTime)
                    .FirstOrDefault()
            };

        return await query.FirstOrDefaultAsync();
    }
    public async Task<List<Machine>> Get(string? search, MachineState? state, int page = 0)
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

                LastReport = reportjoin
                    .OrderByDescending(r => r.CreationTime)
                    .FirstOrDefault()
            };
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e =>

                e.Name.ToLower().Contains(search) ||
                e.Ip.ToLower().Contains(search) ||
                e.Id.ToLower().Contains(search)
            );
        }

        if (state != null)
        {
            query = query.Where(e => (e.LastReport != null) && e.LastReport.State == state);
        }

        return await query.Skip(skipCount).Take(pageSize).ToListAsync();
    }

    public async Task Create(string name, int type, string ip, string site,string manager,string docs)
    {
        var machine = new Machine()
        {
            Id = await GetMachineId(),
            Name = name,
            TypeId = type,
            Ip = ip,
            Site = site,
            Manager = manager,
            docs = docs
            // BootTime = bootTime,
        };
        dbContext.Machines.Add(machine);
        await dbContext.SaveChangesAsync();
    }

    public async Task Update(
        string id,
        string name,
        int type,
        string ip,
        string site,
        string manager,
        string docs)
    {
        var machine = await dbContext.Machines.FindAsync(id);

        if (machine is null)
            return;

        machine.Name = name;
        machine.TypeId = type;
        machine.Ip = ip;
        machine.Site = site;
        machine.Manager = manager;
        machine.docs = docs;

        await dbContext.SaveChangesAsync();
    }

    public async Task Delete(string id)
    {
        var machine = await dbContext.Machines.FindAsync(id) ?? throw new InvalidOperationException();
        dbContext.Machines.Remove(machine);
        await dbContext.SaveChangesAsync();
    }
    public async Task SeedFeatures(){
        var fICMP = new Feature(){Name="ICMP"};
        var fSNMP = new Feature(){Name="SNMP"};
        var fHTTP = new Feature(){Name="HTTP"};
        var fHTTPS = new Feature(){Name="HTTPS"};
        var fMSSQL = new Feature(){Name="MSSQL"};
        var fSSIS = new Feature(){Name="SSIS"};
        var fSSAS = new Feature() { Name = "SSAS" };
        await dbContext.Features.AddRangeAsync([
            fICMP,
            fSNMP,
            fHTTP,
            fHTTPS,
            fMSSQL,
            fSSIS,
            fSSAS
        ]);
        await dbContext.SaveChangesAsync();
    }
    public async Task SeedMachineType(){
        var fICMP = dbContext.Features.Where(f=>   f.Name=="ICMP").First();
        var fSNMP = dbContext.Features.Where(f=>   f.Name=="SNMP").First();
        var fHTTP = dbContext.Features.Where(f=>   f.Name=="HTTP").First();
        var fHTTPS = dbContext.Features.Where(f=>  f.Name=="HTTPS").First();
        var fMSSQL = dbContext.Features.Where(f=>  f.Name=="MSSQL").First();
        var fSSIS = dbContext.Features.Where(f=>   f.Name=="SSIS").First();
        var fSSAS = dbContext.Features.Where(f=>   f.Name== "SSAS").First();
        var mtWebServer = new MachineType() { name = "Serveur Web full", Features = [fICMP,fHTTP,fHTTPS]};
        var mtHttpServer = new MachineType() { name = "Serveur http", Features = [fICMP,fHTTP]};
        var mtHttpsServer = new MachineType() { name = "Serveur https", Features = [fICMP,fHTTPS]};
        var mtSQLServer = new MachineType() { name = "SQL Server", Features = [fICMP,fMSSQL]};
        var mtBIServer = new MachineType() { name = "Serveur BI", Features = [fICMP,fSSIS,fSSAS]};
        var mtSSASServer = new MachineType() { name = "Analysis Services", Features = [fICMP,fSSAS]};
        var mtSSISServer = new MachineType() { name = "Integration Services", Features = [fICMP,fSSIS]};
        var mtDesktop = new MachineType() { name = "Desktop", Features = [fICMP]};
        var mtGeneric = new MachineType() { name = "Serveur Generique", Features = [fICMP, fSNMP] };
        dbContext.MachineTypes.AddRange([
            mtHttpServer,
            mtHttpsServer,
            mtSSISServer,
            mtSSASServer,
            mtWebServer,
            mtSQLServer,
            mtBIServer,
            mtDesktop,
            mtGeneric
        ]);
        await dbContext.SaveChangesAsync();
    }
    public async Task SeedFunctions() {
        var fICMP = dbContext.Features.Where(f=>   f.Name=="ICMP").First();
        var fSNMP = dbContext.Features.Where(f=>   f.Name=="SNMP").First();
        var fHTTP = dbContext.Features.Where(f=>   f.Name=="HTTP").First();
        var fHTTPS = dbContext.Features.Where(f=>  f.Name=="HTTPS").First();
        var fMSSQL = dbContext.Features.Where(f=>  f.Name=="MSSQL").First();
        var fSSIS = dbContext.Features.Where(f=>   f.Name=="SSIS").First();
        var fSSAS = dbContext.Features.Where(f=>   f.Name== "SSAS").First();

        var cfPing = new CalleeFunction("sutnance.hacking", "IPingScanner", "isOnline"){FeatureId=fICMP.Id};
        var cfSnmp = new CalleeFunction("sutnance.hacking", "ISNMPScanner", "FullScan"){FeatureId=fSNMP.Id};
        var cfHttp = new CalleeFunction("sutnance.hacking", "IGenericHttpScanner", "IsHttpRunningAsync"){FeatureId=fHTTP.Id};
        var cfHttps = new CalleeFunction("sutnance.hacking", "IGenericHttpScanner", "IsHttpsRunningAsync"){FeatureId=fHTTPS.Id};
        var cfSql = new CalleeFunction("sutnance.hacking", "ISqlServerScanner", "IsMsSqlRunning"){FeatureId=fMSSQL.Id};
        var cfSsis = new CalleeFunction("sutnance.hacking", "ISqlServerScanner", "IsSsisRunning"){FeatureId=fSSIS.Id};
        var cfSsas = new CalleeFunction("sutnance.hacking", "ISqlServerScanner", "IsSsasRunning") { FeatureId = fSSAS.Id };

        await dbContext.CalleeFunctions.AddRangeAsync([
            cfPing,
            cfSnmp,
            cfHttp,
            cfHttps,
            cfSql,
            cfSsis,
            cfSsas
        ]);
        await dbContext.SaveChangesAsync();
    }
    public async Task SeedMachines(){
        var mtWebServer = dbContext.MachineTypes.Where(m=>m.name == "Serveur Web").First();
        var mtSQLServer = dbContext.MachineTypes.Where(m=>m.name == "SQL Server").First();
        var mtBIServer = dbContext.MachineTypes.Where(m=>m.name == "Serveur BI").First();
        var mtDesktop = dbContext.MachineTypes.Where(m => m.name == "Desktop").First();
        var mtGeneric = dbContext.MachineTypes.Where(m=>m.name == "Serveur Generique").First();
        var mWebServer = new Machine() { Id="EQ-01",Name = "Serveur Web (test)"      ,Type=mtWebServer,Ip="127.0.0.1",docs="http://google.com",Site="Aeroport Houari Boumedien - La DEX"};
        var mSQLServer = new Machine() { Id="EQ-02",Name = "SQL Server (test)"       ,Type=mtSQLServer,Ip="127.0.0.1",docs="http://google.com",Site="Aeroport Houari Boumedien - La DEX"};
        var mBIServer  = new Machine() { Id="EQ-03",Name = "Serveur BI (test)"       ,Type=mtBIServer ,Ip="127.0.0.1",docs="http://google.com",Site="Aeroport Houari Boumedien - La DEX"};
        var mDesktop   = new Machine() { Id="EQ-04",Name = "Desktop (test)"          ,Type=mtDesktop  ,Ip="127.0.0.1",docs="http://google.com",Site="Aeroport Houari Boumedien - La DEX"};
        var mGeneric = new Machine()   { Id="EQ-05",Name = "Serveur Generique (test)", Type = mtGeneric, Ip = "127.0.0.1", docs = "http://google.com", Site = "Aeroport Houari Boumedien - La DEX" };

        await dbContext.Machines.AddRangeAsync([
            mWebServer,
            mSQLServer,
            mBIServer,
            mDesktop,
            mGeneric
        ]);
        await dbContext.SaveChangesAsync();

    }
    public async Task SeedAsync()
    {
        if (await dbContext.Features.AnyAsync())
            return;

        // seeding feature
        // seeding plugings or whatever
        // seeding types
        await this.SeedFeatures();
        await this.SeedFunctions();
        await this.SeedMachineType();
        // await this.SeedMachines();
        // seeding machines

        // var types = new[] { "PC", "Switch", "Modem", "Router", "Server" };
        // var sites = new[] { "Tel Aviv", "La DEX", "Bab Ezzouar", "Algiers", "Oran" };

        // var rnd = new Random();

        // for (int i = 1; i <= 50; i++)
        // {
        //     var type = types[i % types.Length];
        //     var site = sites[i % sites.Length];

        //     var randomDays = rnd.Next(1, 365);
        //     var randomHours = rnd.Next(0, 24);
        //     var randomMinutes = rnd.Next(0, 60);

        //     await this.Create(
        //         name: $"Machine {i}",
        //         type: type,
        //         ip: $"192.168.1.{i}",
        //         site: site
        //     );
        // }

        // await dbContext.SaveChangesAsync();
    }

    /*public async Task SeedReports()
    {
        List<string> machineIds = await dbContext.Machines.Select(m => m.Id).ToListAsync();


    }*/

}
