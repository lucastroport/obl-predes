using System.Net.Sockets;

namespace ProtocolHelper;

public class SocketHandler : ISocketHandler
{
    private readonly Socket _socket;

    public SocketHandler(Socket socket)
    {
        _socket = socket;
    }
    
    public void Send(byte[] buffer)
    {
        int bytesSent = 0;
        int length = buffer.Length;
        
        while (bytesSent < length) {
            int bytesToSend = length - bytesSent;
            var sent = _socket.Send(buffer, bytesSent, bytesToSend, SocketFlags.None);
            if (sent == 0) //Deja de enviar datos antes qye se envien la cantidad esperada
            {
                throw new SocketException();
            }

            bytesSent += sent;
        }
    }

    public byte[] Receive(int length)
    {
        int bytesRead = 0;
        byte[] buffer = new byte[length];
        while (bytesRead < length) {
            int bytesToRead = length - bytesRead;
            var bytesReceived = _socket.Receive(buffer, bytesRead, bytesToRead, SocketFlags.None);
            if (bytesReceived == 0)
            {
                throw new SocketException();
            }
            bytesRead += bytesReceived;
        }
        return buffer;
    }
}