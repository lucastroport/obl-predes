using System.Net;
using System.Net.Sockets;
using System.Text;
using F1;
using log4net;
using log4net.Config;
using ProtocolHelper;
using ProtocolHelper.Communication;

namespace Server;

public class Server
{
    private static readonly int MaxClients = 10;
    private static readonly ILog Logger = LogManager.GetLogger(typeof(Server));
    private static readonly SettingsHelper SettingsHelper = new();
    static Server()
    {
        XmlConfigurator.Configure();
    }
    static void Main()
    {
        // Set the IP address and port for the server
        var ipAddress = IPAddress.Parse(SettingsHelper.ReadSettings(AppConfig.ServerIpConfigKey));
        int port = int.Parse(SettingsHelper.ReadSettings(AppConfig.ServerPortConfigKey));

        var socket = new Socket(
            ipAddress.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        try
        {
            // Bind the listener to the IP address and port
            socket.Bind(new IPEndPoint(ipAddress, port));
            
            // Start listening for incoming client requests
            socket.Listen(MaxClients);
            Console.WriteLine($"Server started in {ipAddress}:{port}. Waiting for connections...");
            Logger.Info($"Server started in {ipAddress}:{port}.");
            
            int connectedClients = 0;
            while (connectedClients < MaxClients)
            {
                var clientSocket = socket.Accept();
                
                var clientRemoteEndpoint = clientSocket.RemoteEndPoint as IPEndPoint;
                
                Console.WriteLine($"Client {clientRemoteEndpoint.Address} connected on port {clientRemoteEndpoint.Port}");
                Logger.Info($"Client with {clientRemoteEndpoint.Address} ip connected to server");
                connectedClients += 1;
                new Thread(() => new ClientHandler(clientSocket).Start()).Start();
            }
        }
        catch (Exception e)
        {
            Logger.Error("Exception: {0}", e);
            Console.Write("Client disconnected");
        }
        finally
        {
            socket.Close();
        }
    }
    
    private class ClientHandler {
        private readonly Socket _socket;

        public ClientHandler(Socket client)
        {
            _socket = client;
        }

        public void Start()
        {
            ISocketHandler handler = new SocketHandler(_socket);
            var clientConnected = true;

            while (clientConnected)
            {
                try
                {
                    var protocolData = new ProtocolData();
                    string logMessage;
                    
                    // Receive the header from the client
                    var headerLength = ByteConvertUtils.MessageByteLength(Constants.RequestHeader);
                    var bytesLength = handler.Receive(headerLength);
                    var header = handler.Receive(BitConverter.ToInt32(bytesLength));
                    var parsedHeader = ParseClientResponse(header);
                    protocolData.Header = parsedHeader;
                    logMessage = $"Header received: {parsedHeader}";
                    Console.WriteLine(logMessage);
                    Logger.Info(logMessage);

                    // Receive the operation from the client
                    var operationLength = ByteConvertUtils.MessageByteLength(Constants.Operation);
                    bytesLength = handler.Receive(operationLength);
                    var operation = handler.Receive(BitConverter.ToInt32(bytesLength));
                    var parsedOperation = int.Parse(ParseClientResponse(operation));
                    protocolData.Operation = parsedOperation;
                    logMessage = $"Operation received: {parsedOperation}";
                    Console.WriteLine(logMessage);
                    Logger.Info(logMessage);

                    // Receive the query from the client
                    bytesLength = handler.Receive(Constants.FixedLength);
                    var query = handler.Receive(BitConverter.ToInt32(bytesLength));
                    var parsedQuery = ParseClientResponse(query);
                    protocolData.Query = QueryDataSerializer.Deserialize(parsedQuery);
                    logMessage = $"Query received: {parsedQuery}";
                    Console.WriteLine(logMessage);
                    Logger.Info(logMessage);

                    // Acknowledge each field to the client
                    SendAck(handler, Constants.ResponseHeader);
                    SendAck(handler, Constants.OperationReceived);
                    SendAck(handler, Constants.QueryReceived);

                    // Process the data and generate a response
                    var response = "Data received and processed successfully";
                    SendAck(handler, protocolData.ToString());
                }
                catch (Exception e)
                {
                    clientConnected = false;
                    Logger.Error("Exception:", e);
                    Console.Write("Client disconnected");
                }
            }
        }
        private static string ParseClientResponse(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        private static void SendAck(ISocketHandler handler, string ack)
        {
            byte[] responseBytes = Encoding.ASCII.GetBytes(ack);
            byte[] responseLengthBytes = BitConverter.GetBytes(responseBytes.Length);
            handler.Send(responseLengthBytes);

            // Send the response to the client
            handler.Send(responseBytes);
        }
        
    }
}