using System.Net.NetworkInformation;
namespace sutnance.hacking;

public interface IPingScanner
{
    Task<bool> isOnline(string ip);
}
public class PingScanner : IPingScanner
{
    private int timeoutMilliseconds = 2000;
    public async Task<bool> isOnline(string ip)
    {

        try
        {
            using Ping pinger = new Ping();
            PingReply reply = await pinger.SendPingAsync(ip, timeoutMilliseconds);

            return reply.Status == IPStatus.Success;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
