using System.Net.Sockets;

namespace ProtocolHelper;

public class SocketHandler : ISocketHandler
{
    private readonly Socket _socket;

    public SocketHandler(Socket socket)
    {
        _socket = socket;
    }
    
    public int Send(byte[] data, int length)
    {
        int bytesSent = 0;
        while (bytesSent < length) {
            int bytesToSend = Math.Min(256, length - bytesSent);
            bytesSent += _socket.Send(data, bytesSent, bytesToSend, SocketFlags.None);
        }
        return bytesSent;
    }

    public int Receive(byte[] data, int length)
    {
        int bytesRead = 0;
        while (bytesRead < length) {
            int bytesToRead = Math.Min(256, length - bytesRead);
            bytesRead += _socket.Receive(data, bytesRead, bytesToRead, SocketFlags.None);
        }
        return bytesRead;
    }
}