namespace ProtocolHelper;

public interface ISocketHandler
{
    void Send(byte[] data);
    byte[] Receive(int length);
}