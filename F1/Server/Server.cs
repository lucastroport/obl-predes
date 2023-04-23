using System.Net;
using System.Net.Sockets;
using System.Text;
using F1;
using F1.Constants;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;
using log4net;
using log4net.Config;
using Microsoft.VisualBasic;
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
    private static Dictionary<Socket, string> _connectedClients = new ();
    private static Socket _clientSocket;
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
            
            while (_connectedClients.Count < MaxClients)
            {
                _clientSocket = socket.Accept();
                _connectedClients.Add(_clientSocket,"");
                
                var clientRemoteEndpoint = _clientSocket.RemoteEndPoint as IPEndPoint;
                
                Console.WriteLine($"Client {clientRemoteEndpoint.Address} connected on port {clientRemoteEndpoint.Port}");
                Logger.Info($"Client with {clientRemoteEndpoint.Address} ip connected to server");
                new Thread(() => new ClientHandler(_clientSocket).Start()).Start();
            }
        }
        catch (Exception e)
        {
            Logger.Error("Exception: {0}", e);
            Console.WriteLine("Client disconnected");
        }
        finally
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            Logger.Info("Socket closed");
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
            
            while (clientConnected)
            {
                try
                {
                    var protocolData = protocolProcessor.Process();
                    // Process the data and generate a response
                    bool containsFilename = false;
                    bool containsFileSize = false;
                    
                    if (protocolData.Query != null)
                    {
                        containsFilename = protocolData.Query.Fields.TryGetValue(ConstantKeys.FileNameKey, out var filename);
                        containsFileSize = protocolData.Query.Fields.TryGetValue(ConstantKeys.FileSizeKey, out var fileSizeRaw);
                        protocolData.Query.Fields.TryGetValue(ConstantKeys.PartKey, out var partId);

                        if (containsFilename && containsFileSize)
                        {
                            var fileCommonHandler = new FileCommsHandler(_socket);
                            var writePath = fileCommonHandler.ReceiveFile(long.Parse(fileSizeRaw), filename);
                            var partRepository = PartRepository.Instance;
                            var part = partRepository.QueryById(partId);
                            if (part != null)
                            {
                                part.PhotoUrl = writePath;
                            }
                            var response = new ProtocolData(
                                false,
                                protocolData.Operation,
                                new QueryData
                                {
                                    Fields = new Dictionary<string, string>
                                    {
                                        { "RESULT", "File transferred correctly" },
                                        { "MENU",  MenuOptions.ListItems(MenuOptions.MenuItems)}
                                    }
                                }
                            );
                            protocolProcessor.Send(
                                $"{response.Header}" +
                                $"{response.Operation}" +
                                response.QueryLength +
                                $"{QueryDataSerializer.Serialize(response.Query)}"
                                );
                        }

                    }
                    if (!containsFilename && !containsFileSize)
                    {
                        ProcessData(protocolProcessor, protocolData, _socket); 
                    }
                    
                }
                catch (Exception e)
                {
                    clientConnected = false;
                    Logger.Error("Exception:", e);
                    Console.WriteLine("Client disconnected");
                }
            }
        }
        
        private static void ProcessData(ProtocolProcessor processor, ProtocolData data, Socket socket)
        {
            var menu = new Menu();
            var operationHandler = new OperationHandler();

            if (IsClientAuthenticated())
            {
                menu.TriggerLoggedInMenu();
                if (data.Query != null)
                {
                    var loggedUser = _connectedClients[_clientSocket];
                    data.Query.Fields.Add(ConstantKeys.Authenticated, loggedUser);    
                }
            }
            else
            {
                menu.TriggerNotAuthMenu();
            }
            
            var response = operationHandler
                .HandleMenuAction(
                    int.Parse(data.Operation), 
                    data.Query != null ? new CommandQuery(data.Query.Fields) : null,
                    menu
                );
            var userAuthenticated = response.Query.Fields.TryGetValue(ConstantKeys.Authenticated, out var username);
            var isLogout = response.Query.Fields.ContainsKey(ConstantKeys.Logout);
            var isSendFile = response.Query.Fields.TryGetValue(ConstantKeys.SendFileUrlKey, out var fileUrl);

            if (isSendFile)
            {
                var fileCommonHandler = new FileCommsHandler(socket);
                fileCommonHandler.SendFile(fileUrl, response);
            }
            else
            {
                if (isLogout)
                {
                    if (_connectedClients.ContainsKey(_clientSocket))
                    {
                        _connectedClients.Remove(_clientSocket);
                    }
                }
                if (userAuthenticated)
                {
                    if (_connectedClients.ContainsKey(_clientSocket))
                    {
                        _connectedClients[_clientSocket] = username;
                    }
                    else
                    {
                        _connectedClients.Add(_clientSocket,username);
                    }
                }
            
                var ack = $"{response.Header}" +
                          $"{response.Operation}" +
                          response.QueryLength +
                          $"{QueryDataSerializer.Serialize(response.Query)}";
                processor.Send(ack);    
            }
        }
    }

    private static bool IsClientAuthenticated()
    {
        var containsClient = _connectedClients.TryGetValue(_clientSocket, out var username);
        return containsClient && !string.IsNullOrEmpty(username);
    }
}