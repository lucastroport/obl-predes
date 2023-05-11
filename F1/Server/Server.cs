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
    private static readonly int BackLog = 100;
    private static readonly ILog Logger = LogManager.GetLogger(typeof(Server));
    private static readonly SettingsHelper SettingsHelper = new();
    private static Dictionary<TcpClient, string> _connectedClients = new();
    private static readonly object AuthUserReadLock = new();
    private static readonly object AuthAddLock = new();
    private static readonly object ReadFieldsLock = new();
    private static readonly object SendFileLock = new();

    static Server()
    {
        XmlConfigurator.Configure();
    }

    static async Task Main()
    {
        var ipAddress = IPAddress.Parse(SettingsHelper.ReadSettings(AppConfig.ServerIpConfigKey));
        var port = int.Parse(SettingsHelper.ReadSettings(AppConfig.ServerPortConfigKey));
        var adminPassword = SettingsHelper.ReadSettings(AppConfig.AdminPasswordKey);
        UserRepository.Instance.AddUser(new User("admin", adminPassword, UserType.Admin));

        var tcpClient = new TcpClient();
        try
        {
            var tcpListener = new TcpListener(new IPEndPoint(ipAddress, port));

            tcpListener.Start(BackLog);
            Console.WriteLine($"Server started in {ipAddress}:{port}. Waiting for connections...");
            Logger.Info($"Server started in {ipAddress}:{port}.");

            
            while (true)
            {
                tcpClient = await tcpListener.AcceptTcpClientAsync();
                _connectedClients.Add(tcpClient, "");
                
                var clientRemoteEndpoint = tcpClient.Client.RemoteEndPoint as IPEndPoint;

                Console.WriteLine(
                    $"Client {clientRemoteEndpoint.Address} connected on port {clientRemoteEndpoint.Port}");
                Logger.Info($"Client with {clientRemoteEndpoint.Address} ip connected to server");
                var clientHandler = new ClientHandler(tcpClient);
                Task.Run(async () => await clientHandler.Start());
            }
        }
        catch (Exception e)
        {
            Logger.Error("Exception: {0}", e);
            if (_connectedClients.ContainsKey(tcpClient))
            {
                var user = UserRepository.Instance.QueryByUsername(_connectedClients[tcpClient]);
                if (user != null)
                {
                    user.IsLoggedIn = false;
                }
            }
            Console.WriteLine("Client disconnected");
        }
        finally
        {
            tcpClient.GetStream().Close();
            tcpClient.Close();
            Logger.Info("TcpClient closed");
        }
    }

    private class ClientHandler
    {
        private readonly TcpClient _tcpClient;

        public ClientHandler(TcpClient client)
        {
            _tcpClient = client;
        }

        public async Task Start()
        {
            var handler = new NetworkHandler(_tcpClient);
            var protocolProcessor = new ProtocolProcessor(handler);
            var clientConnected = true;

            while (clientConnected)
            {
                try
                {
                    var protocolData = await protocolProcessor.Process();
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
                            var fileCommonHandler = new FileCommsHandler(_tcpClient);
                            var writePath = await fileCommonHandler.ReceiveFile(long.Parse(fileSizeRaw), filename);
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
                        ProcessData(protocolProcessor, protocolData, _tcpClient);
                    }
                }
                catch (Exception e)
                {
                    var user = UserRepository.Instance.QueryByUsername(_connectedClients[_tcpClient]);
                    if (user != null)
                    {
                        user.IsLoggedIn = false;
                    }
                    clientConnected = false;
                    Logger.Error("Exception:", e);
                    Console.WriteLine("Client disconnected");
                }
            }
        }
        private static bool IsClientAuthenticated(TcpClient tcpClient)
        {
            var containsClient = _connectedClients.TryGetValue(tcpClient, out var username);
            return containsClient && !string.IsNullOrEmpty(username);
        }
        private static void ProcessData(ProtocolProcessor processor, ProtocolData data, TcpClient tcpClient)
        {
            var menu = new Menu();
            var operationHandler = new OperationHandler();

            if (IsClientAuthenticated(tcpClient))
            {
                menu.TriggerLoggedInMenu();
                if (data.Query != null)
                {
                    var loggedUser = _connectedClients[tcpClient];
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
                connectedClient = _connectedClients.TryGetValue(tcpClient, out var client)
                    ? client
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
                    var fileCommonHandler = new FileCommsHandler(tcpClient);
                    fileCommonHandler.SendFile(fileUrl, response);   
                }
            }
            else
            {
                if (isLogout)
                {
                    if (_connectedClients.ContainsKey(tcpClient))
                    {
                        var user = UserRepository.Instance.QueryByUsername(_connectedClients[tcpClient]);
                        if (user != null)
                        {
                            user.IsLoggedIn = false;
                        }
                        _connectedClients.Remove(tcpClient);
                    }
                }

                lock (AuthAddLock)
                {
                    if (userAuthenticated)
                    {
                        _connectedClients[tcpClient] = username;
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