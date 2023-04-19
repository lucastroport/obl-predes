using System.Net;
using System.Net.Sockets;
using System.Text;
using F1;
using F1.Constants;
using F1.Presentation.Views.Menu;
using log4net;
using log4net.Config;
using ProtocolHelper;
using ProtocolHelper.Communication;
using ProtocolHelper.Communication.Models;
using Server.Commands;
using Server.Handlers;

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
            var handler = new SocketHandler(_socket);
            var protocolProcessor = new ProtocolProcessor(handler);
            var clientConnected = true;
            
            // When client connected send main menu to client
            // ProcessData(protocolProcessor, new ProtocolData(
            //     true,
            //     $"{MenuItemConstants.MainMenu}",
            //     null
            // ));
            
            while (clientConnected)
            {
                try
                {
                    var protocolData = protocolProcessor.Process();
                    // Process the data and generate a response
                    ProcessData(protocolProcessor, protocolData);
                }
                catch (Exception e)
                {
                    clientConnected = false;
                    Logger.Error("Exception:", e);
                    Console.Write("Client disconnected");
                }
            }
        }
        

        private static void ProcessData(ProtocolProcessor processor, ProtocolData data)
        {
            var menu = new Menu();
            var operationHandler = new OperationHandler();
            menu.TriggerNotAuthMenu();
            
            var response = operationHandler
                .HandleMenuAction(
                    int.Parse(data.Operation), 
                    data.Query != null ? new CommandQuery(data.Query.Fields) : null,
                    menu
                );

            var ack = $"{response.Header}" +
                      $"{response.Operation}" +
                      response.QueryLength +
                      $"{QueryDataSerializer.Serialize(response.Query)}";
            processor.SendAck(ack);
        }
    }
}