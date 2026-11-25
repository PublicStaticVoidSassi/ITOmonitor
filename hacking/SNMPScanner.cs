using System.Globalization;
using System.Net;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
namespace sutnance.hacking;

public record SNMPResult(
        float TotalRam,
        float UsedRam,
        float FreeRam,
        float CPUUsage,
        TimeSpan Uptime
        );

public interface ISNMPScanner
{
    SNMPResult FullScan(string address);
    // float GetTotalRam();
    // float GetUsedRam();
    // float GetFreeRam();
    // float GetCPUUsage();
    // TimeSpan GetUptime();
}

public class SNMPScanner : ISNMPScanner
{
    private const string SysUpTimeOid =
        "1.3.6.1.2.1.1.3.0";
    private const string HrMemorySizeOid =
        "1.3.6.1.2.1.25.2.2.0";

    private const string HrStorageTableOid =
        "1.3.6.1.2.1.25.2.3";

    private const string HrStorageTypeOid =
        "1.3.6.1.2.1.25.2.3.1.2";

    private const string HrStorageAllocationUnitsOid =
        "1.3.6.1.2.1.25.2.3.1.4";

    private const string HrStorageSizeOid =
        "1.3.6.1.2.1.25.2.3.1.5";

    private const string HrStorageUsedOid =
        "1.3.6.1.2.1.25.2.3.1.6";

    private const string HrProcessorTableOid =
        "1.3.6.1.2.1.25.3.3";

    private const string HrProcessorLoadOid =
        "1.3.6.1.2.1.25.3.3.1.2";

    // HOST-RESOURCES-MIB::hrStorageRam
    private const string HrStorageRamType =
        "1.3.6.1.2.1.25.2.1.2";

    private IPEndPoint _endpoint;
    private readonly OctetString _community;
    private readonly int _timeout;

    public SNMPScanner(
        string community = "public",
        int timeout = 5000)
    {
        _community = new OctetString(community);
        _timeout = timeout;
    }

    // ---------------------------------------------------------
    // Basic GET
    // ---------------------------------------------------------

    private Variable Get(string oid)
    {
        var variables = new List<Variable>
        {
            new(new ObjectIdentifier(oid))
        };

        var result = Messenger.Get(
            VersionCode.V2,
            _endpoint,
            _community,
            variables,
            _timeout);

        if (result.Count == 0)
            throw new InvalidOperationException(
                $"SNMP returned no data for OID {oid}.");

        return result[0];
    }

    // ---------------------------------------------------------
    // Walk
    // ---------------------------------------------------------

    private List<Variable> Walk(string oid)
    {
        var result = new List<Variable>();

        Messenger.BulkWalk(
            VersionCode.V2,
            _endpoint,
            _community,
            OctetString.Empty,
            new ObjectIdentifier(oid),
            result,
            _timeout,
            25,
            WalkMode.WithinSubtree,
            null,
            null);

        return result;
    }

    // ---------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------

    private static long GetLong(Variable variable)
    {
        if (long.TryParse(
                variable.Data.ToString(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var value))
        {
            return value;
        }

        throw new InvalidOperationException(
            $"SNMP value '{variable.Data}' is not an integer.");
    }

    private static string GetIndex(string oid, string columnOid)
    {
        if (!oid.StartsWith(columnOid + "."))
            throw new InvalidOperationException(
                $"OID '{oid}' does not belong to '{columnOid}'.");

        return oid[(columnOid.Length + 1)..];
    }
    //
    // uptime
    //
    public TimeSpan GetUptime()
    {
        try {
            var value = Get(SysUpTimeOid);
            long ticks = GetLong(value);
            return TimeSpan.FromMilliseconds(ticks * 10);
        }catch{
            return new TimeSpan();
        }
    }
    // ---------------------------------------------------------
    // RAM
    // ---------------------------------------------------------

    public float GetTotalRam()
    {
        /*
         * hrMemorySize is defined in KBytes.
         *
         * Convert to MB before returning.
         */
        try {
            var value = Get(HrMemorySizeOid);

            long kilobytes = GetLong(value);

            return kilobytes / 1024f;
        }catch{
            return 0;
        }
    }

    public float GetUsedRam()
    {
        try {
            var storage = GetRamStorage();
            return storage.UsedBytes / 1024f / 1024f;
        }catch{
            return 0;
        }
    }

    public float GetFreeRam()
    {
        try {
            var storage = GetRamStorage();
            return storage.FreeBytes / 1024f / 1024f;
        }catch{
            return 0;
        }
    }

    private RamStorage GetRamStorage()
    {
        /*
         * hrStorageTable looks roughly like:
         *
         * hrStorageType.<index>
         * hrStorageAllocationUnits.<index>
         * hrStorageSize.<index>
         * hrStorageUsed.<index>
         *
         * We first find the row whose type is hrStorageRam.
         */

        var types = Walk(HrStorageTypeOid);

        string? ramIndex = null;

        foreach (var variable in types)
        {
            if (variable.Data.ToString() == HrStorageRamType)
            {
                ramIndex = GetIndex(
                    variable.Id.ToString(),
                    HrStorageTypeOid);

                break;
            }
        }

        if (ramIndex == null)
            throw new InvalidOperationException(
                "Windows SNMP agent did not expose hrStorageRam.");

        var allocationUnits = Get(
            $"{HrStorageAllocationUnitsOid}.{ramIndex}");

        var size = Get(
            $"{HrStorageSizeOid}.{ramIndex}");

        var used = Get(
            $"{HrStorageUsedOid}.{ramIndex}");

        long units = GetLong(allocationUnits);
        long totalUnits = GetLong(size);
        long usedUnits = GetLong(used);

        long totalBytes = units * totalUnits;
        long usedBytes = units * usedUnits;

        return new RamStorage(
            totalBytes,
            usedBytes,
            totalBytes - usedBytes);
    }

    private readonly record struct RamStorage(
        long TotalBytes,
        long UsedBytes,
        long FreeBytes);

    // ---------------------------------------------------------
    // CPU
    // ---------------------------------------------------------

    public float GetCPUUsage()
    {
        try {
            var processors = Walk(HrProcessorLoadOid);
            float total = 0;
            foreach (var processor in processors)
            {
                total += GetLong(processor);
            }
            return total / processors.Count;
        }catch{
            return 0;
        }
    }

    public SNMPResult FullScan(string address)
    {
        this._endpoint = new IPEndPoint(IPAddress.Parse(address), 161);
        return new SNMPResult(
            this.GetTotalRam(),
            this.GetUsedRam(),
            this.GetFreeRam(),
            this.GetCPUUsage(),
            this.GetUptime()
        );
    }
}
