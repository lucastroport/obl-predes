using System.Net.Sockets;

namespace ProtocolHelper;

public class NetworkHandler : INetworkHandler
{
    private readonly TcpClient _tcpClient;

    public NetworkHandler(TcpClient client)
    {
        _tcpClient = client;
    }
    
    public async Task Send(byte[] buffer)
    {
        var stream = _tcpClient.GetStream();
        int length = buffer.Length;

        await stream.WriteAsync(buffer, 0, length);
    }

    public async Task<Byte[]> Receive(int length, int receivedLength = 0)
    {
        var stream = _tcpClient.GetStream();
        var offSet = receivedLength;
        byte[] buffer = new byte[length];
        while (offSet < length) {
            int bytesToRead = length - offSet;
            var bytesReceived = await stream.ReadAsync(buffer, offSet, bytesToRead);
            if (bytesReceived == 0)
            {
                throw new SocketException();
            }
            offSet += bytesReceived;
        }
        return buffer;
    }
}