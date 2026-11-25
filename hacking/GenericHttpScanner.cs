// despite its name, it also scans https

namespace sutnance.hacking;
public interface IGenericHttpScanner
{
    Task<bool> IsHttpRunningAsync(string ip);
    Task<bool> IsHttpsRunningAsync(string ip);
}

public class GenericHttpScanner(INetCat netCat) : IGenericHttpScanner
{
    public async Task<bool> IsHttpRunningAsync(string ip)
    {
        return await netCat.CanConnectAsync(ip, 80);
    }
    public async Task<bool> IsHttpsRunningAsync(string ip)
    {
        return await netCat.CanConnectAsync(ip, 443);
    }
}
