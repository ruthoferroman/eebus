
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
    const string MSG_FORMAT = "JSON-UTF8";
    // maximum allowed are 30 seconds, according to spec
    private static readonly TimeSpan _CmiTimeout = TimeSpan.FromSeconds(30);
    // maximum alowed are 240 seconds, according to the spec
    private static readonly TimeSpan _HelloTimeout = TimeSpan.FromSeconds(240);

    private readonly ILogger<ShipWebSocketClient> _logger;
    private readonly ClientWebSocket _webSocket;
    private readonly string _ski;
    private readonly string _uri;

    public ShipWebSocketClient(ILogger<ShipWebSocketClient> logger, ClientWebSocket webSocket, string uri, string ski)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(webSocket);
        this._logger = logger;
        this._webSocket = webSocket;
        this._ski = ski;
        this._uri = uri;
        logger.BeginScope("{Uri}", uri);
    }

    public async Task ConnectAsync(X509Certificate2 localCert, CancellationToken cancellationToken)
    {
        // 1. Configure TLS (Mutual Auth)
        _webSocket.Options.ClientCertificates.Add(localCert);

        // SHIP usually requires a specific sub-protocol: "ship"
        _webSocket.Options.AddSubProtocol("ship");

        // Remote certificate validation (Trust logic)
        _webSocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
        {
            string? ski = certificate.GetSubjectKeyIdentifier();
            if (_ski.ToLower() != ski?.ToLower())
            {
                _logger.LogInformation("Not trusted {@ski}", ski);
                return false;
            }
            _logger.LogInformation("RemoteCertificateValidationCallback {@ski}",ski);
            // TODO Here you would check the remote device's SKI against your "Trusted" list
            return true; // Simplified for this example
        };

  
        _logger.LogInformation("Connecting ...");
        await _webSocket.ConnectAsync(new Uri(_uri), cancellationToken);

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
        await ShipHandshakePhase(linkedHandshakeCts.Token);
    }

    public async Task DataExchange(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Data Exchange ...");
        while (_webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            using var reader = await _webSocket.ReceiveMessageAsync(cancellationToken);
            var msg = DataValueEncoder.Decode(reader, (int)reader.BaseStream.Length)
                ?? throw new ShipException("Failed to decode SHIP Data message.");

            // TODO
            _logger.LogInformation("Received message: {@msg}", msg);

        }
    }

    private async Task ShipInitPhase(CancellationToken cancellationToken)
    {
        await _webSocket.SendMessageAsync((writer) =>
        {
            writer.Write((byte)ShipMessageType.Init);
            writer.Write((byte)ShipMessageValue.CmiHead);
        }, cancellationToken);
        _logger.LogInformation("Init Sent.");

        using (var msg = await _webSocket.ReceiveMessageAsync(cancellationToken))
        {
            var initResponse = msg.ReadBytes(2);
            if ((initResponse[0] != (byte)ShipMessageType.Init) || (initResponse[1] != (byte)ShipMessageValue.CmiHead))
                throw new ShipException("Expected init response message!");

            _logger.LogInformation("Received Init Response: {@response}", initResponse);
        }
    }


    private async Task ShipHelloPhase(CancellationToken cancellationToken)
    {
        // send initial Hello message
        var request = new SmeHelloValue(

               new ConnectionHelloType(ConnectionHelloPhaseType.Ready, (uint)_HelloTimeout.TotalSeconds, null, null)
           );
        await _webSocket.SendMessageAsync(request.Encode, cancellationToken);
        _logger.LogInformation("--> Sent SHIP Hello {@req}", request);


        // wait for Hello response and handle phases
        while (!cancellationToken.IsCancellationRequested)
        {
            using var msg = await _webSocket.ReceiveMessageAsync(cancellationToken);
            var resp = SmeHelloValueEncoder.Decode(msg, (int)msg.BaseStream.Length)
                ?? throw new ShipException("Failed to decode SHIP Hello message.");
            var helloResponse = resp.ConnectionHello
                ?? throw new ShipException("SHIP Hello response doesn't contain ConnectionHello element.");

            _logger.LogInformation("<-- Received SHIP Hello {@resp}", resp);

            // validate phase and handle accordingly
            switch (helloResponse.Phase)
            {
                case ConnectionHelloPhaseType.Ready:
                    return; // Hello exchange complete
                case ConnectionHelloPhaseType.Pending:
                    if (helloResponse.ProlongationRequest == true)
                    {
                        _logger.LogInformation("Received SHIP Hello Phase 'Pending' with Prolongation Request with waiting {Waiting}ms, will send new Hello.", helloResponse.Waiting);
                        // TODO Token
                        //cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                        var prHelloMsg = new SmeHelloValue(
                           
                               new ConnectionHelloType(ConnectionHelloPhaseType.Pending, helloResponse.Waiting, null, null)
                           );
                        await _webSocket.SendMessageAsync(prHelloMsg.Encode, cancellationToken);
                    }
                    else
                    {
                        _logger.LogInformation("Received SHIP Hello Phase 'Pending', waiting for {Waiting} ms.", helloResponse.Waiting);
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
        await _webSocket.SendMessageAsync(abortMsg.Encode, cancellationToken);
    }

    private async Task ShipHandshakePhase(CancellationToken cancellationToken)
    {
        var locVersion = new MessageProtocolHandshakeTypeVersion(1, 0);
        
        // send handshake message
        var request = new SmeProtocolHandshakeValue( new MessageProtocolHandshakeType(ProtocolHandshakeTypeType.AnnounceMax, locVersion, [MSG_FORMAT]) );
        await _webSocket.SendMessageAsync(request.Encode, cancellationToken);
        _logger.LogInformation("Handshake Sent {@msg}", request);

        // receive handshake response
        using var response = await _webSocket.ReceiveMessageAsync(cancellationToken);
        // TODO Check for error message response
        var handshakeResponse = SmeProtocolHandshakeEcnoder.Decode(response, (int)response.BaseStream.Length)
            ?? throw new ShipException("Failed to decode SHIP Handshake response message.");

        if (handshakeResponse is SmeProtocolHandshakeErrorValue ev)
            throw new ShipException($"SHIP Handshake error response received {ev.MessageProtocolHandshakeError.Error}");
        else if (handshakeResponse is SmeProtocolHandshakeValue hv)
        {
            // validate response
            // TODO - send Handshake error response if validation fails
            var first = hv.MessageProtocolHandshake//.FirstOrDefault()
                    ?? throw new ShipException("SHIP Handshake response doesn't contain elements.");
            if (first.HandshakeType == ProtocolHandshakeTypeType.Select)
                throw new ShipException("SHIP Handshake response Protocol version selection expected!");

            var firstVersion = first.Version
                ?? throw new ShipException("SHIP Handshake response doesn't contain protocol versions.");

            if (firstVersion != locVersion)
                throw new ShipException($"SHIP Handshake response Protocol version mismatch! Expected: {locVersion}, Received: {firstVersion}");

            if (!first.Formats.Any(v => v == MSG_FORMAT))
                throw new ShipException($"SHIP Handshake response doesn't contain supported protocol format {MSG_FORMAT}");
        }
        else
            throw new ShipException("Unexpected SHIP Handshake response message type.");
    }

}



