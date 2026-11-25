// lol this is just a tcpip connect test
using System.Net.Sockets;
namespace sutnance.hacking;
public interface INetCat {
    Task<bool> CanConnectAsync(string ip, int port, int timeout = 2000);
}
public class NetCat : INetCat
{
    public async Task<bool> CanConnectAsync(string ip, int port, int timeout = 2000)
    {
        try
        {
            using (var client = new TcpClient())
            {
                var connectTask = client.ConnectAsync(ip, port);
                // Enforce a strict timeout so the script doesn't hang indefinitely
                var delayTask = Task.Delay(timeout);

                var completedTask = await Task.WhenAny(connectTask, delayTask);
                if (completedTask == connectTask && client.Connected)
                {
                    return true;
                }
            }
        }
        catch
        {
            // Fail silently; port is closed or blocked
        }
        return false;
    }
}
