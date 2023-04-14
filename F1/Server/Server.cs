using System.Net;
using System.Net.Sockets;
using System.Text;
using ProtocolHelper;

namespace Server;

internal class Server
{
    private static int maxClients = 10;
    
    static void Main()
    {
        // Set the IP address and port for the server
        var ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 20000;

        var socket = new Socket(
            ipAddress.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        try
        {
            // Bind the listener to the IP address and port
            socket.Bind(new IPEndPoint(ipAddress, port));
            
            // Start listening for incoming client requests
            socket.Listen(maxClients);
            Console.WriteLine("Server started. Waiting for connections...");

            int connectedClients = 0;
            while (connectedClients < maxClients)
            {
                var clientSocket = socket.Accept();
                Console.Write("Client connected!");
                connectedClients += 1;
                new Thread(() => new ClientHandler(clientSocket).Start()).Start();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e);
        }
        finally
        {
            socket.Close();
        }
    }
    
    private class ClientHandler {
        private Socket _socket;

        public ClientHandler(Socket client)
        {
            _socket = client;
        }

        public void Start()
        {
            ISocketHandler handler = new SocketHandler(_socket);
            var clientConntected = true;
            
            while (clientConntected)
            {
                try {
                    // Receive data from the client
                    var bytesLength = handler.Receive(Constants.FixedLength);
                    var data = handler.Receive(BitConverter.ToInt32(bytesLength));

                    Console.WriteLine("Client said: {0}", Encoding.UTF8.GetString(data));
                }
                catch (Exception e)
                {
                    clientConntected = false;
                    Console.WriteLine("Exception: {0}", e);
                }
            }
        }
    }
}