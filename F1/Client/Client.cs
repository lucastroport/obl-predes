using System.Net;
using System.Net.Sockets;
using F1;
using log4net;
using ProtocolHelper;
using ProtocolHelper.Communication;
using ProtocolHelper.Communication.Models;
using Constants = F1.Constants;

namespace Client;

internal class Client
{
    private static readonly SettingsHelper SettingsHelper = new();
    private static readonly ILog Logger = LogManager.GetLogger(typeof(Client));
    private static readonly object SendFileLock = new();
    static async Task Main()
    {
        var ipAddress = IPAddress.Parse(SettingsHelper.ReadSettings(AppConfig.ClientIpConfigKey));
        var serverAddress = IPAddress.Parse(SettingsHelper.ReadSettings(AppConfig.ServerIpConfigKey));
        
        int serverPort = int.Parse(SettingsHelper.ReadSettings(AppConfig.ServerPortConfigKey));

        var tcpClient = new TcpClient(new IPEndPoint(ipAddress, 0)); 

        try
        {
            INetworkHandler handler;
            ProtocolProcessor protocolProcessor;
            ProtocolData response;

            await tcpClient.ConnectAsync(serverAddress, serverPort);

            Console.WriteLine("Connected to server");
            Logger.Info($"Connected to server on {serverAddress} port {serverPort}");

            handler = new NetworkHandler(tcpClient);

            var protocolData = new ProtocolData(
                true,
                $"{Constants.MenuItemConstants.MainMenu}",
                null
            );
            var serializedQuery = QueryDataSerializer.Serialize(protocolData.Query);
            protocolProcessor = new ProtocolProcessor(handler);

            protocolProcessor.Send(
                $"{protocolData.Header}" +
                $"{protocolData.Operation}" +
                $"{protocolData.QueryLength}" +
                $"{serializedQuery}"
            );

            response = await protocolProcessor.Process();
            
            string message = "";

            var menu = "";
            var isMenu = response.Query != null && response.Query.Fields.TryGetValue("MENU", out menu);

            if (isMenu)
            {
                if (isMenu)
                {
                    Console.WriteLine(menu);
                }
            }

            var waitingResponse = false;
            while (!message.Equals($"{Constants.MenuItemConstants.ExitMenu}"))
            {
                if (!waitingResponse)
                {
                    var correct = false;
                    while (!correct)
                    {
                        message = Console.ReadLine();
                        var parsed = int.TryParse(message, out var numOp);
                        correct = parsed && numOp is >= 0 and <= 99;
                        if (!correct)
                        {
                            Console.WriteLine("Please enter a valid number option.");
                            Console.WriteLine(menu);
                        }
                    }

                    protocolData = new ProtocolData(
                        true,
                        message,
                        null
                    );

                    serializedQuery = QueryDataSerializer.Serialize(protocolData.Query);

                    protocolProcessor.Send(
                        $"{protocolData.Header}" +
                        $"{protocolData.Operation}" +
                        $"{protocolData.QueryLength}" +
                        $"{serializedQuery}"
                    );
                }

                // Receive and print the server response
                response = await protocolProcessor.Process();

                var result = "";
                var filename = "";
                var fileSizeRaw = "";
                if (response.Query != null) isMenu = response.Query.Fields.TryGetValue("MENU", out menu);
                var containsResult = response.Query != null && response.Query.Fields.TryGetValue("RESULT", out result);
                var isSaveArchive = response.Query != null && response.Query.Fields.ContainsKey(Constants.ConstantKeys.SaveFileKey);
                var containsFilename = response.Query != null && response.Query.Fields.TryGetValue(Constants.ConstantKeys.FileNameKey, out filename);
                var containsFileSize = response.Query != null && response.Query.Fields.TryGetValue(Constants.ConstantKeys.FileSizeKey, out fileSizeRaw);
                
                if (isSaveArchive)
                {
                    lock (SendFileLock)
                    {
                        var fileCommonHandler = new FileCommsHandler(tcpClient);
                        response.Query.Fields.TryGetValue(Constants.ConstantKeys.SaveFileKey, out var filePath);
                        fileCommonHandler.SendFile(filePath, response);   
                    }
                }
                else
                {
                    if (containsFilename && containsFileSize)
                    {
                        var fileCommonHandler = new FileCommsHandler(tcpClient);
                        var writePath = await fileCommonHandler.ReceiveFile(long.Parse(fileSizeRaw), filename);
                        Console.WriteLine($"File downloaded in {writePath}");  
                    }
                    
                    if (isMenu || containsResult)
                    {
                        if (containsResult)
                        {
                            Console.WriteLine(result);
                        }

                        if (isMenu)
                        {
                            Console.WriteLine(menu);
                        }

                        waitingResponse = false;
                    }
                    else
                    {
                        var queryFields = new Dictionary<string, string>();
                        // We ask the client for fields to complete and send to the server
                        string input = "";
                        foreach (KeyValuePair<string, string> pair in response.Query.Fields)
                        {
                            while (string.IsNullOrEmpty(input))
                            {
                                Console.WriteLine($"{pair.Value} {pair.Key}:");
                                input = Console.ReadLine();
                                if (string.IsNullOrEmpty(input))
                                {
                                    Console.WriteLine($"{pair.Key} cannot be empty");
                                }
                            }

                            if (!string.IsNullOrEmpty(input))
                            {
                                queryFields.Add(pair.Key, input);
                                input = "";
                            }
                        }

                        protocolData = new ProtocolData(
                            true,
                            response.Operation,
                            new QueryData { Fields = queryFields }
                        );

                        serializedQuery = QueryDataSerializer.Serialize(protocolData.Query);

                        protocolProcessor.Send(
                            $"{protocolData.Header}" +
                            $"{protocolData.Operation}" +
                            $"{protocolData.QueryLength}" +
                            $"{serializedQuery}"
                        );
                        waitingResponse = true;
                    }
                }
            }
        }
        catch (Exception e)
        {
            tcpClient.GetStream().Close();
            tcpClient.Close();
            
            Logger.Error("Exception: {0}", e);
        }
    }
}