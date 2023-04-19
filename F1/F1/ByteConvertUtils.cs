using System.Text;

namespace F1;

public static class ByteConvertUtils
{
    public static int MessageByteLength(string message)
    {
        byte[] messageBytes = Encoding.ASCII.GetBytes(message);
        return BitConverter.GetBytes(messageBytes.Length).Length;
    }
}