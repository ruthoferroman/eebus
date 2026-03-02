
using eebus.Spine;
using Makaretu.Dns;
using System.Net;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;


namespace eebus.Spine;

/// <summary>
/// </summary>
/// <param name="logger"></param>
/// <param name="webSocket"></param>
internal class SpineWebsocketClient
{
    private readonly ILogger<SpineWebsocketClient> _logger;
    private readonly ClientWebSocket _webSocket;
    private readonly JsonSerializerOptions serializerOptions = new()
    {
        Converters =
            {
                new DatagramJsonConverter()
            }
    };
    public SpineWebsocketClient(ILogger<SpineWebsocketClient> logger, ClientWebSocket webSocket, string uri)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(webSocket);
        this._logger = logger;
        this._webSocket = webSocket;

        logger.BeginScope("{Uri}", uri);
    }

   

    public async Task DataExchange(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Data Exchange ...");
        while (_webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            using var reader = await _webSocket.ReceiveMessageAsync(CancellationToken.None);
            var msg = DataValueEncoder.Decode(reader, (int)reader.BaseStream.Length)
                ?? throw new SpineException("Failed to decode SHIP Data message.");

            foreach (var data in msg.Data)
            {
                var payload = Encoding.UTF8.GetString(data.Payload);
                var datagram = JsonSerializer.Deserialize<DatagramType>(payload, serializerOptions);
                _logger.LogInformation("Received message: {@payload}", payload);
            }
            // TODO
       

        }
    }

    

}
