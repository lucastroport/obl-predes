using System.Net;
using System.Net.Sockets;
using F1;
using F1.Constants;
using F1.Domain.Model;
using F1.Domain.Repository;
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
    private static Dictionary<Socket, string> _connectedClients = new();
    private Socket _clientSocket;
    private static readonly object AuthUserReadLock = new();
    private static readonly object AuthAddLock = new();
    private static readonly object ReadFieldsLock = new();
    private static readonly object SendFileLock = new();
    private static readonly object SocketLock = new();

    static Server()
    {
        XmlConfigurator.Configure();
    }

    static void Main()
    {
        // Set the IP address and port for the server
        var ipAddress = IPAddress.Parse(SettingsHelper.ReadSettings(AppConfig.ServerIpConfigKey));
        int port = int.Parse(SettingsHelper.ReadSettings(AppConfig.ServerPortConfigKey));
        var adminPassword = SettingsHelper.ReadSettings(AppConfig.AdminPasswordKey);
        UserRepository.Instance.AddUser(new User("admin", adminPassword, UserType.Admin));

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
                var clientSocket = socket.Accept();
                _connectedClients.Add(clientSocket, "");
                
                var clientRemoteEndpoint = clientSocket.RemoteEndPoint as IPEndPoint;

                Console.WriteLine(
                    $"Client {clientRemoteEndpoint.Address} connected on port {clientRemoteEndpoint.Port}");
                Logger.Info($"Client with {clientRemoteEndpoint.Address} ip connected to server");
                new Thread(() => new ClientHandler(clientSocket).Start()).Start();
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

    private class ClientHandler
    {
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
                        containsFilename =
                            protocolData.Query.Fields.TryGetValue(ConstantKeys.FileNameKey, out var filename);
                        containsFileSize =
                            protocolData.Query.Fields.TryGetValue(ConstantKeys.FileSizeKey, out var fileSizeRaw);
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
                                        { "MENU", MenuOptions.ListItems(MenuOptions.MenuItems) }
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
        private static bool IsClientAuthenticated(Socket socket)
        {
            var containsClient = _connectedClients.TryGetValue(socket, out var username);
            return containsClient && !string.IsNullOrEmpty(username);
        }
        private static void ProcessData(ProtocolProcessor processor, ProtocolData data, Socket socket)
        {
            var menu = new Menu();
            var operationHandler = new OperationHandler();

            if (IsClientAuthenticated(socket))
            {
                menu.TriggerLoggedInMenu();
                if (data.Query != null)
                {
                    var loggedUser = _connectedClients[socket];
                    data.Query.Fields.Add(ConstantKeys.Authenticated, loggedUser);
                }
            }
            else
            {
                menu.TriggerNotAuthMenu();
            }

            string connectedClient;
            lock (AuthUserReadLock)
            {
                connectedClient = _connectedClients.ContainsKey(socket)
                    ? _connectedClients[socket]
                    : "";
            }
            var response = operationHandler
                .HandleMenuAction(
                    int.Parse(data.Operation),
                    data.Query != null ? new CommandQuery(data.Query.Fields) : null,
                    menu,
                    connectedClient
                );
            bool userAuthenticated;
            bool isLogout;
            bool isSendFile;
            string fileUrl;
            string username;
            
            lock (ReadFieldsLock)
            {
                userAuthenticated = response.Query.Fields.TryGetValue(ConstantKeys.Authenticated, out username);
                isLogout = response.Query.Fields.ContainsKey(ConstantKeys.Logout);
                isSendFile = response.Query.Fields.TryGetValue(ConstantKeys.SendFileUrlKey, out fileUrl);
    
            }
            if (isSendFile)
            {
                lock (SendFileLock)
                {
                    var fileCommonHandler = new FileCommsHandler(socket);
                    fileCommonHandler.SendFile(fileUrl, response);   
                }
            }
            else
            {
                if (isLogout)
                {
                    if (_connectedClients.ContainsKey(socket))
                    {
                        var user = UserRepository.Instance.QueryByUsername(_connectedClients[socket]);
                        if (user != null)
                        {
                            user.IsLoggedIn = false;
                        }
                        _connectedClients.Remove(socket);
                    }
                }

                lock (AuthAddLock)
                {
                    if (userAuthenticated)
                    {
                        if (_connectedClients.ContainsKey(socket))
                        {
                            _connectedClients[socket] = username;
                        }
                        else
                        {
                            _connectedClients.Add(socket, username);
                        }
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
}