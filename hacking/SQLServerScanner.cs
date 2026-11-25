using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace sutnance.hacking;
interface ISqlServerScanner
{
    Task<bool> IsMsSqlRunning(string ip);
    Task<bool> IsSsasRunning(string ip);
    Task<bool> IsSsisRunning(string ip);
}

class SqlServerScanner(INetCat netCat) : ISqlServerScanner
{
    private const int SqlServerPort = 1433;
    private const int SsasPort = 2383;
    private const int SsisPort = 135;

    public async Task<bool> IsMsSqlRunning(string ip)
    {
        return await netCat.CanConnectAsync(ip, SqlServerPort);
    }
    public async Task<bool> IsSsasRunning(string ip)
    {
        return await netCat.CanConnectAsync(ip, SsasPort);
    }
    public async Task<bool> IsSsisRunning(string ip)
    {
        return await netCat.CanConnectAsync(ip, SsisPort);
    }
}
