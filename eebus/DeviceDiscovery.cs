using Makaretu.Dns;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;

internal class DeviceDiscovery(ILogger<DeviceDiscovery> logger)
{
    private readonly ConcurrentQueue<EEBusDevice> eEBusDevices = new();
    public async IAsyncEnumerable<EEBusDevice> DiscoverAsync()
    {
        logger.LogInformation("Discovering services on the local network...");

        using (var mdns = new MulticastService())

        // Create an mDNS service discovery instance
        using (var sd = new ServiceDiscovery(mdns))
        {
            mdns.Start();

            // Event handler for when a service instance is resolved
            sd.ServiceInstanceDiscovered += (s, e) =>
            {
                using (logger.BeginScope("{ServiceInstanceName} {RemoteEndPoint}", e.ServiceInstanceName, e.RemoteEndPoint))
                {
                    var servers = e.Message.Answers.OfType<SRVRecord>();
                    var addresses = e.Message.Answers.OfType<AddressRecord>();
                    var txt = e.Message.Answers.OfType<TXTRecord>();
                    foreach (var server in servers)
                    {
                        logger.LogInformation($"  SRV: {server.Target}:{server.Port}");
                        foreach (var address in addresses)
                        {
                            eEBusDevices.Enqueue(new EEBusDevice(e.ServiceInstanceName.ToString(), address.Address, server.Port, txt.SelectMany(t => t.Strings).Select(v => v.Split("=")).ToDictionary(v => v[0], v => v[1])));
                        }
                    }
                }
            };

            sd.QueryServiceInstances("_ship._tcp");

            // Keep running for 10 seconds to capture responses
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        if (eEBusDevices.IsEmpty)
        {
            logger.LogWarning("No devices found.");
            yield break;
        }
        while(eEBusDevices.TryDequeue(out var device))
        {
            yield return device;
        }
        logger.LogInformation("Discovery finished.");
    }
}

internal record EEBusDevice(string Name, IPAddress Address, int Port, Dictionary<string,string> TxtRecords);
