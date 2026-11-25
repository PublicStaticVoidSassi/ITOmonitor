using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.PortableExecutable;
using Microsoft.EntityFrameworkCore;
using sutnance.hacking;

namespace sutnance.Models;

public enum MachineState
{
    Critique = 1,
    Stable = 2,
    Partial = 3
}

public abstract class AuditedTable
{
    public DateTime CreationTime { get; init; } = DateTime.Now;  // gonna add more if needed
}

[PrimaryKey(nameof(Id))]
public class Note: AuditedTable {
    public int Id { get; set; }
    public string Content { get; set; }
    public string HistoriqueId { get; set; }
}

[PrimaryKey(nameof(Id))]
public class Alert: AuditedTable
{
    public int Id { get; set; }
    public string Details { get; set; } = "Machine is down";
    // public MachineState State { get; set; } = MachineState.Critique;
    public string machineId { get; set; }
    public bool resolved { get; set; } = false;

    public Alert(string machineId)
    {
        this.machineId = machineId;
    }
}

// [PrimaryKey(nameof(Id))]
// public class comprenant
// {
//     public int Id { get; set; }
//     public string? characteristique { get; set; }
//     public int Max { get; set; }
//     public int Min { get; set; }
// }
[PrimaryKey(nameof(Id))]
public class MachineType {
    public int Id { get; set; }
    public required string name { get; set; }
    public List<Feature> Features { get; set; } = default!;
}
[PrimaryKey(nameof(Id))]
public class Feature { // characteristic
    public int Id  {get;set;}
    public string Name { get; set; }
    public List<CalleeFunction> Functions { get; set; }
    // public int MachineTypeId { get; set; }
}
public class CalleeFunction(string nameSpace, string className, string functionName, string assemblyName = default!)
{
    public int Id  { get; set; }
    public string? AssemblyName { get; set; } = assemblyName;
    public string NameSpace { get; set; } = nameSpace;
    public string ClassName { get; set; } = className;
    public string FunctionName { get; set; } = functionName;
    public int FeatureId { get; set; }
    // public Feature Feature { get; set; }
}
[PrimaryKey(nameof(Id))]
public class Machine : AuditedTable
{

    public string Id { get; set; } // EQ-001
    public string Name { get; set; }
    public int TypeId { get; set; }
    public MachineType Type { get; set; }
    public string Ip { get; set; }
    public string Site { get; set; } // location not website lol
    public string Manager { get; set; } = "sasicraft4@gmail.com";
    public string? docs { get; set; } = "https://www.example.com";


    [NotMapped]
    public Report? LastReport { get; set; }

    [NotMapped]
    public string? LastCheck
    {
        get
        {
            if (LastReport is null)
                return null;

            TimeSpan diff = DateTime.Now - LastReport.CreationTime;
            // return diff.ToString();
            if (diff.TotalDays >= 1)
            {
                int days = (int)diff.TotalDays;
                return $"Il y a {days} jour{(days > 1 ? "s" : "")}";
            }

            if (diff.TotalHours >= 1)
            {
                int hours = (int)diff.TotalHours;
                return $"Il y a {hours} heure{(hours > 1 ? "s" : "")}";
            }

            if (diff.TotalMinutes >= 1)
            {
                int minutes = (int)diff.TotalMinutes;
                return $"Il y a {minutes} minute{(minutes > 1 ? "s" : "")}";
            }

            int seconds = Math.Max(0, (int)diff.TotalSeconds);
            return $"Il y a {seconds} seconde{(seconds > 1 ? "s" : "")}";
        }
    }
}

    [PrimaryKey(nameof(Id))]
    public class Report : AuditedTable
    {

        public string Id { get; set; } // R-001
        public MachineState State { get; set; } // from ping
        public string MachineId { get; set; }
        public Machine Machine { get; set; }

        // SNMP stuff (NotAlwaysSupported)
        public float TotalRam { get; set; } = default;
        public float UsedRam { get; set; } = default;
        public float FreeRam { get; set; } = default;
        public float CPUUsage { get; set; } = default;
        public TimeSpan Uptime { get; set; } = default;

        // SQL Stuff
        public bool? IsSqlServerRunning { get; set; } = default;
        public bool? IsSsisRunning { get; set; } = default;
        public bool? IsSsasRunning { get; set; } = default;
        // HTTP Stuff
        public bool? IsHttpRunning { get; set; } = default;
        public bool? IsHttpsRunning { get; set; } = default;

        public void AddSNMPResults(SNMPResult res)
        {
            this.TotalRam = res.TotalRam;
            this.UsedRam = res.UsedRam;
            this.FreeRam = res.FreeRam;
            this.CPUUsage = res.CPUUsage;
            this.Uptime = res.Uptime;
        }


    }


// [PrimaryKey(nameof(Id))]
// public class Historique : AuditedTable
// {
//     public string Id { get; set; } // H-1024
//     public string Title { get; set; }
//     public string? TraitePar { get; set; }

//     /*[ForeignKey(nameof(Machine))]*/
//     public string MachineId { get; set; }
//     /*[ForeignKey(nameof(Report))] WHAT THE !#@% */
//     public string ReportId { get; set; }

//     public Machine Machine { get; set; }
//     public Report Report { get; set; }

// }
