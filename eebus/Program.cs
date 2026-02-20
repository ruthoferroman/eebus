// See https://aka.ms/new-console-template for more information
using eebus;
using eebus.Ship;
using Microsoft.Extensions.Logging.Console;
using System.Net;


var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole(options =>
    {
        // Use SimpleConsoleFormatter
        options.FormatterName = ConsoleFormatterNames.Simple;
    });
    builder.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true; // Show scopes if used
        options.SingleLine = true;    // One-line log entries
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss "; // Custom timestamp
    });
});

var logger = loggerFactory.CreateLogger<Program>();

var cert = CertGenerator.CreateSelfSignedCertificate(Dns.GetHostName(),"1111");
string? ski = cert.GetSubjectKeyIdentifier();

var dsvc = new DeviceDiscoveryService(ski,12480);
dsvc.Run();

var tmp = new DeviceDiscovery(loggerFactory.CreateLogger<DeviceDiscovery>());
var devices = await tmp.DiscoverAsync().ToListAsync();
foreach (var dev in devices)
    logger.LogInformation("device found: {Device}", dev);



while (true)
{
    try
    {
        var websocket = new System.Net.WebSockets.ClientWebSocket();

        ShipWebSocketClient client = new(loggerFactory.CreateLogger<ShipWebSocketClient>(), websocket, "wss://192.168.1.152:12480", "1aa91a8f21869234ad5860f4fddadd4267fa4a0e");
        await client.ConnectAsync(cert, CancellationToken.None);
        await client.DataExchange(CancellationToken.None);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while connecting to the SHIP device.");
    }
}

