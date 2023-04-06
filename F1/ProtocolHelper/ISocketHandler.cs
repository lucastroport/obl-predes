namespace ProtocolHelper;

public interface ISocketHandler
{
    int Send(byte[] data, int length);
    int Receive(byte[] data, int length);
}