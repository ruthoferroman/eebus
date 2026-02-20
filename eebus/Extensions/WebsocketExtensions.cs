using System.Buffers;
using System.Net.WebSockets;
using System.Text;

namespace eebus.Extensions;

internal static class WebsocketExtensions
{
    public static async Task<BinaryReader> ReceiveMessageAsync(this WebSocket webSocket, CancellationToken cancellationToken)
    {
        ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        var buffer = pool.Rent(1024 * 4);

        var ms = new MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            result = await webSocket.ReceiveAsync(buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
                throw new WebSocketException("Connection closed by server during receive.");

            ms.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);

        ms.Seek(0, SeekOrigin.Begin);
        return new BinaryReader(ms, Encoding.UTF8);
    }
        
    public static async Task SendMessageAsync(this WebSocket webSocket, Action<BinaryWriter> encode, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
        {
            encode(writer);
            ms.Seek(0, SeekOrigin.Begin);
            await webSocket.SendAsync(ms.ToArray(), WebSocketMessageType.Binary, true, cancellationToken);
        }
    }
}
