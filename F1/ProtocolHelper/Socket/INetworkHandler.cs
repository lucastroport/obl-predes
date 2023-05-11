namespace ProtocolHelper;

public interface INetworkHandler
{
    Task Send(byte[] data);
    Task<byte[]> Receive(int length, int receivedLength = 0);
}