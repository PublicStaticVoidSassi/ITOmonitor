using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using sutnance.Data;
using sutnance.Models;

namespace sutnance.hacking;

public class Monitor(IServiceProvider sp) : BackgroundService
{

    private async Task aMinute()
    { // get it????? xdxdxddxdxdx
        await Task.Delay(TimeSpan.FromMinutes(1));
    }
    private MachineState GetMachineState(Machine machine,Dictionary<string, object> res ){
        List<string> functionnames = [.. machine.Type
            .Features
            .SelectMany(f=>f.Functions)
            .Select(f=>$"{f.ClassName}.{f.FunctionName}")];
        if (!((bool)res["IPingScanner.isOnline"])){
            return MachineState.Critique;
        }
        foreach(var f in functionnames)
        {
            object obj = res[f];
            if (obj is bool bobj){
                if (!bobj){
                    return MachineState.Partial;
                }
            }
        };
        return MachineState.Stable;
    }
    private async Task<Report> GenerateReport(
        Machine machine, Dictionary<string, object> res)
    {
        using var scope = sp.CreateScope();
        var reportManager = scope.ServiceProvider.GetRequiredService<ReportManager>();
        SNMPResult? snmpresults = (SNMPResult?)(res.GetValueOrDefault("ISNMPScanner.FullScan"));
        bool isonline = (bool)res.GetValueOrDefault("IPingScanner.isOnline")!;
        bool? httpOn = (bool?)res.GetValueOrDefault("IGenericHttpScanner.IsHttpRunningAsync");
        bool? httpsOn = (bool?)res.GetValueOrDefault("IGenericHttpScanner.IsHttpsRunningAsync");
        bool? ssisOn = (bool?)res.GetValueOrDefault("ISqlServerScanner.IsSsisRunning");
        bool? ssasOn = (bool?)res.GetValueOrDefault("ISqlServerScanner.IsSsasRunning");
        bool? mssqlOn = (bool?)res.GetValueOrDefault("ISqlServerScanner.IsMsSqlRunning");
        var report = await reportManager.makeReport(machine.Id, GetMachineState(machine, res));
        if (snmpresults != null) report.AddSNMPResults(snmpresults);
        report.IsHttpRunning = httpOn;
        report.IsHttpsRunning = httpsOn;
        report.IsSqlServerRunning = mssqlOn;
        report.IsSsasRunning = ssasOn;
        report.IsSsisRunning = ssisOn;
        return report;
    }

    public async Task MonitorAsync()
    {
        using var scope = sp.CreateScope();
        // Type t = Type.GetType("sutnance.Data.sutnanceContext")!;
        var context = scope.ServiceProvider.GetRequiredService<sutnanceContext>();
        var codeExecutor = scope.ServiceProvider.GetRequiredService<ICodeExecutor>();
        // var reportManager = scope.ServiceProvider.GetRequiredService<ReportManager>();
        // var pingScanner = scope.ServiceProvider.GetRequiredService<IPingScanner>();
        // var SNMPScanner = scope.ServiceProvider.GetRequiredService<ISNMPScanner>();
        var AlertsManager = scope.ServiceProvider.GetRequiredService<IAlertsManager>();
        var machines = await context.Machines.AsNoTracking()
        .Include(m=>m.Type)
        .ThenInclude(t=>t.Features)
        .ThenInclude(f=>f.Functions)
        .ToListAsync();
        foreach (var machine in machines)
        {
            bool machineOffline = false;
            Dictionary<string, object> FunctionResults = [];
            Console.WriteLine($"We are at machine {machine.Name}");
            foreach (var feature in machine.Type.Features)
            {
                Console.WriteLine($"\tWe are at feature {feature.Name}");
                if (machineOffline)
                {
                    continue;
                }
                foreach (var func in feature.Functions)
                {
                    Console.WriteLine($"\t\tWe are at function {func.ClassName}.{func.FunctionName}");
                    Console.WriteLine("\t\tExecuting....");
                    try
                    {
                        object ret = await codeExecutor.RunFunctionAsync(func.NameSpace, func.ClassName, func.FunctionName, [machine.Ip], func.AssemblyName);
                        Console.WriteLine("\t\tReturned: " + JsonSerializer.Serialize(ret, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }));
                        // if the ping scan fails. no need to waste time on the other scans
                        if (func.FunctionName == "isOnline" && !(bool)ret)
                        {
                            machineOffline = true;
                        }
                        FunctionResults.Add($"{func.ClassName}.{func.FunctionName}", ret);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"\t\tException thrown: {e.Message}");
                        // throw e;
                    }

                }
            }
            Console.WriteLine($"Scan for machine '{machine.Name}' has ended");
            Console.WriteLine(JsonSerializer.Serialize(FunctionResults, new JsonSerializerOptions() { WriteIndented = true }));
            Report report = await this.GenerateReport(machine, FunctionResults);
            if (report.State != MachineState.Stable)
            {
                await AlertsManager.RaiseAlertIfNeeded(machine);
            }
            context.Reports.Add(report);
            context.SaveChanges();
            // MachineState _state;
            // if (await pingScanner.isOnline(machine.Ip))
            // {
            //     _state = MachineState.Stable;
            // }
            // else
            // {
            //     _state = MachineState.Critique;
            //     AlertsManager.RaiseAlertIfNeeded(machine);
            // }

            // Console.WriteLine($"{machine.Id} : {_state}");
            // var report = await reportManager.makeReport(machine.Id, _state);
            // try
            // {
            //     report.AddSNMPResults(SNMPScanner.FullScan(machine.Ip));
            // }
            // catch (Exception e)
            // {
            //     Console.WriteLine($"snmp scan failed on {machine.Id}");
            // }

            // await context.Reports.AddAsync(report);
            // await context.SaveChangesAsync();
        }
        // Console.WriteLine("The scanning is over");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await this.MonitorAsync();
            await aMinute();
        }
    }
}
