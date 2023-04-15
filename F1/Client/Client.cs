using System.Net;
using System.Net.Sockets;
using System.Text;
using ProtocolHelper;

namespace Client;

internal class Client
{
    static void Main()
    {
        var ipAddress = IPAddress.Parse("127.0.0.1");
        int serverPort = 20000;
        
        var client = new Socket(
            ipAddress.AddressFamily, 
            SocketType.Stream, 
            ProtocolType.Tcp);

        try
        {
            client.Bind(new IPEndPoint(ipAddress, 0));
            client.Connect(new IPEndPoint(ipAddress, serverPort));
            Console.Write("Connected to server");

            var exit = false;
            ISocketHandler handler = new SocketHandler(client);
            var message = "";
            
            while (!message.Equals("exit"))
            {
                message = Console.ReadLine();

                // Get the data to send to the server
                byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                
                // Send the length of the data first
                byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
                handler.Send(lengthBytes);

                // Send the actual data to the server
                handler.Send(messageBytes);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            client.Close();
        }
    }
}