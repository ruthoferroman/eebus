using eebus.Extensions;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;


namespace eebus.Ship;

/// <summary>
/// SHIP (Smart Home IP):
/// SHIP is a protocol that allows devices in a smart home environment to communicate over IP networks.
/// It provides a framework for implementing various use cases related to energy management and device interoperability.
/// </summary>
/// <param name="logger"></param>
/// <param name="webSocket"></param>
internal class ShipWebSocketClient
{
    // maximum allowed are 30 seconds, according to spec
    private static readonly TimeSpan _CmiTimeout = TimeSpan.FromSeconds(30);
    // maximum alowed are 240 seconds, according to the spec
    private static readonly TimeSpan _HelloTimeout = TimeSpan.FromSeconds(240);
    private readonly ILogger<ShipWebSocketClient> logger;
    private readonly ClientWebSocket webSocket;
    private readonly string uri;

    public ShipWebSocketClient(ILogger<ShipWebSocketClient> logger, ClientWebSocket webSocket, string uri)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(webSocket);
        this.logger = logger;
        this.webSocket = webSocket;
        this.uri = uri;
        logger.BeginScope("{Uri}", uri);
    }

    public async Task ConnectAsync(X509Certificate2 localCert, CancellationToken cancellationToken)
    {
        // 1. Configure TLS (Mutual Auth)
        webSocket.Options.ClientCertificates.Add(localCert);

        // SHIP usually requires a specific sub-protocol: "ship"
        webSocket.Options.AddSubProtocol("ship");

        // Remote certificate validation (Trust logic)
        webSocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
        {
            logger.LogInformation("RemoteCertificateValidationCallback");
            // TODO Here you would check the remote device's SKI against your "Trusted" list
            return true; // Simplified for this example
        };

  
        logger.LogInformation("Connecting ...");
        await webSocket.ConnectAsync(new Uri(uri), cancellationToken);

        // 1. init
        var initCts = new CancellationTokenSource(_CmiTimeout);
        using var linkedInitCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, initCts.Token);
        await ShipInitPhase(linkedInitCts.Token);

        // 2. hello
        var helloCts = new CancellationTokenSource(_HelloTimeout);
        using var linkedHelloCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, helloCts.Token);
        await ShipHelloPhase(linkedHelloCts.Token);

        // 3. version, format... handshake
        var handshakeCts = new CancellationTokenSource(_CmiTimeout);
        using var linkedHandshakeCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, handshakeCts.Token);
        await ShipHsPhase(linkedHandshakeCts.Token);
    }

    public async Task DataExchange(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Data Exchange ...");
        while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            using var reader = await webSocket.ReceiveMessageAsync(cancellationToken);
            var msg = DataValueEncoder.Decode(reader, (int)reader.BaseStream.Length)
                ?? throw new ShipException("Failed to decode SHIP Data message.");

            // TODO
            logger.LogInformation("Received message: {@msg}", msg);

        }
    }

    private async Task ShipInitPhase(CancellationToken cancellationToken)
    {
        await webSocket.SendMessageAsync((writer) =>
        {
            writer.Write((byte)ShipMessageType.Init);
            writer.Write((byte)ShipMessageValue.CmiHead);
        }, cancellationToken);
        logger.LogInformation("Init Sent.");

        using (var msg = await webSocket.ReceiveMessageAsync(cancellationToken))
        {
            var initResponse = msg.ReadBytes(2);
            if ((initResponse[0] != (byte)ShipMessageType.Init) || (initResponse[1] != (byte)ShipMessageValue.CmiHead))
                throw new ShipException("Expected init response message!");

            logger.LogInformation("Received Init Response: {@response}", initResponse);
        }
    }


    private async Task ShipHelloPhase(CancellationToken cancellationToken)
    {
        // send initial Hello message
        var request = new SmeHelloValue(

               new ConnectionHelloType(ConnectionHelloPhaseType.Ready, (uint)_HelloTimeout.TotalSeconds, null, null)
           );
        await webSocket.SendMessageAsync(request.Encode, cancellationToken);
        logger.LogInformation("--> Sent SHIP Hello {@req}", request);


        // wait for Hello response and handle phases
        while (!cancellationToken.IsCancellationRequested)
        {
            using var msg = await webSocket.ReceiveMessageAsync(cancellationToken);
            var resp = SmeHelloValueEncoder.Decode(msg, (int)msg.BaseStream.Length)
                ?? throw new ShipException("Failed to decode SHIP Hello message.");
            var helloResponse = resp.ConnectionHello
                ?? throw new ShipException("SHIP Hello response doesn't contain ConnectionHello element.");

            logger.LogInformation("<-- Received SHIP Hello {@resp}", resp);

            // validate phase and handle accordingly
            switch (helloResponse.Phase)
            {
                case ConnectionHelloPhaseType.Ready:
                    return; // Hello exchange complete
                case ConnectionHelloPhaseType.Pending:
                    if (helloResponse.ProlongationRequest == true)
                    {
                        logger.LogInformation("Received SHIP Hello Phase 'Pending' with Prolongation Request with waiting {Waiting}ms, will send new Hello.", helloResponse.Waiting);
                        // TODO Token
                        //cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                        var prHelloMsg = new SmeHelloValue(
                           
                               new ConnectionHelloType(ConnectionHelloPhaseType.Pending, helloResponse.Waiting, null, null)
                           );
                        await webSocket.SendMessageAsync(prHelloMsg.Encode, cancellationToken);
                    }
                    else
                    {
                        logger.LogInformation("Received SHIP Hello Phase 'Pending', waiting for {Waiting} ms.", helloResponse.Waiting);
                        //await Task.Delay((int)(helloResponse.Waiting ?? 30000), cancellationToken);
                    }
                    break;
                case ConnectionHelloPhaseType.Aborted:
                    throw new ShipException("SHIP Hello Phase 'Aborted' Aborted from server.");
            }




        }

        // send abort if we run into timeout
        var abortMsg = new SmeHelloValue(

               new ConnectionHelloType(ConnectionHelloPhaseType.Aborted, null, null, null)
           );
        await webSocket.SendMessageAsync(abortMsg.Encode, cancellationToken);
    }

    private async Task ShipHsPhase(CancellationToken cancellationToken)
    {
        var msgFormat = "JSON-UTF8";
        var locVersion = new MessageProtocolHandshakeTypeVersion(1, 0);
        
        // send handshake message
        var handShakeMessage = new SmeProtocolHandshakeValue(
            
                new MessageProtocolHandshakeType(ProtocolHandshakeTypeType.AnnounceMax, locVersion, [msgFormat])
            );
        await webSocket.SendMessageAsync(handShakeMessage.Encode, cancellationToken);
        logger.LogInformation("Handshake Sent {@msg}", handShakeMessage);

        // receive handshake response
        using var handshakeResponseMsg = await webSocket.ReceiveMessageAsync(cancellationToken);
        // TODO Check for error message response
        var handshakeResponse = SmeProtocolHandshakeValueEncoder.Decode(handshakeResponseMsg, (int)handshakeResponseMsg.BaseStream.Length)
            ?? throw new ShipException("Failed to decode SHIP Handshake response message.");

        // validate response
        // TODO - send Handshake error response if validation fails
        var first = handshakeResponse.MessageProtocolHandshake//.FirstOrDefault()
                ?? throw new ShipException("SHIP Handshake response doesn't contain elements.");
        if (first.HandshakeType == ProtocolHandshakeTypeType.Select)
                 throw new ShipException("SHIP Handshake response Protocol version selection expected!");

        var firstVersion = first.Version
            ?? throw new ShipException("SHIP Handshake response doesn't contain protocol versions.");

        if (firstVersion != locVersion)
            throw new ShipException($"SHIP Handshake response Protocol version mismatch! Expected: {locVersion}, Received: {firstVersion}");

        if ( !first.Formats.Any(v => v == msgFormat))
           throw new ShipException($"SHIP Handshake response doesn't contain supported protocol format {msgFormat}");
    }

}



