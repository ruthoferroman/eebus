using Makaretu.Dns;
namespace eebus;

internal class DeviceDiscoveryService
{
    private readonly ServiceProfile serviceProfile;
  
        public DeviceDiscoveryService(string ski, ushort port = 50000)
    {
        serviceProfile = new("RR_Test_EEBUS_Gateway", "_ship._tcp", port);
        serviceProfile.AddProperty("model", "RR 1");
        serviceProfile.AddProperty("type", "RR 1");
        serviceProfile.AddProperty("brand", "RR 1");
        serviceProfile.AddProperty("ski", ski);
        serviceProfile.AddProperty("register", "false");
        serviceProfile.AddProperty("path", "/ship/");
        serviceProfile.AddProperty("id", "RR 1safdfsadf");
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

