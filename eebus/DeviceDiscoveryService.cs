using Makaretu.Dns;
using System.Net;
namespace eebus;

internal class DeviceDiscoveryService
{
    private readonly ServiceProfile serviceProfile;
  
        public DeviceDiscoveryService(string ski, ushort port = 50000)
    {
        serviceProfile = new($"Test_EEBUS_Gateway_{Dns.GetHostName()}", "_ship._tcp", port);
        serviceProfile.AddProperty("model", "RR 1");
        serviceProfile.AddProperty("type", "Some test device type");
        serviceProfile.AddProperty("brand", "No Brand");
        serviceProfile.AddProperty("ski", ski);
        serviceProfile.AddProperty("register", "false");
        serviceProfile.AddProperty("path", "/ship/");
        serviceProfile.AddProperty("id", $"RR_{Dns.GetHostName()}");
    }
    public void AddProperty(string key, string value)
    {
        serviceProfile.AddProperty(key, value);
    }

    public void Run()
    {
        _ = Task.Run(async () =>
        {
            Thread.CurrentThread.IsBackground = true;

            MulticastService mdns = new MulticastService();
            ServiceDiscovery sd = new ServiceDiscovery(mdns);

            try
            {
                mdns.Start();

                sd.Advertise(serviceProfile);
                sd.Announce(serviceProfile);

                await Task.Delay(-1).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                sd.Dispose();
                mdns.Stop();
            }
        });
    }
}

