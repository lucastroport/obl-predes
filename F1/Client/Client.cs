using System.Net;
using System.Net.Sockets;
using System.Text;
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
    static void Main()
    {
        var ipAddress = IPAddress.Parse(SettingsHelper.ReadSettings(AppConfig.ClientIpConfigKey));
        var serverAddress = IPAddress.Parse(SettingsHelper.ReadSettings(AppConfig.ServerIpConfigKey));;
        int serverPort = int.Parse(SettingsHelper.ReadSettings(AppConfig.ServerPortConfigKey));;
        
        var client = new Socket(
            ipAddress.AddressFamily, 
            SocketType.Stream, 
            ProtocolType.Tcp);

        try
        {
            client.Bind(new IPEndPoint(ipAddress, 0));
            client.Connect(new IPEndPoint(serverAddress, serverPort));

            Console.WriteLine($"Connected to server");
            Logger.Info($"Connected to server on {serverAddress} port {serverPort}");
            ISocketHandler handler = new SocketHandler(client);

            var protocolData = new ProtocolData(
                true,
                $"{Constants.MenuItemConstants.MainMenu}",
                null
            );
            var serializedQuery = QueryDataSerializer.Serialize(protocolData.Query);
            SendPacket(handler, 
                $"{protocolData.Header}" +
                $"{protocolData.Operation}" +
                $"{protocolData.QueryLength}" +
                $"{serializedQuery}"
            );
            string message="";
            var protocolProcessor = new ProtocolProcessor(handler);
            var response = protocolProcessor.Process();
            var menu = "";
            var isMenu = response.Query != null && response.Query.Fields.TryGetValue("MENU", out menu);
            if (isMenu)
            {
                Console.WriteLine(menu);
            }
            
            while (!message.Equals($"{Constants.MenuItemConstants.ExitMenu}"))
            {
                message = Console.ReadLine();
                
                protocolData = new ProtocolData(
                    true,
                    message,
                    null
                );
                serializedQuery = QueryDataSerializer.Serialize(protocolData.Query);
                
                SendPacket(handler, 
                    $"{protocolData.Header}" +
                    $"{protocolData.Operation}" +
                    $"{protocolData.QueryLength}" +
                    $"{serializedQuery}"
                );
                
                // Receive and print the server response
                response = protocolProcessor.Process();
                if (response.Query != null) isMenu = response.Query.Fields.TryGetValue("MENU", out menu);
                if (isMenu)
                {
                    Console.WriteLine(menu);
                }
                else
                {
                    foreach (KeyValuePair<string, string> pair in response.Query.Fields) {
                        Console.WriteLine("Key: {0}, Value: {1}", pair.Key, pair.Value);
                    }   
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error("Exception: {0}", e);
            client.Close();
        }
    }
    
    private static void SendPacket(ISocketHandler handler, string message)
    {
        // Send the length of the data first
        byte[] messageBytes = Encoding.ASCII.GetBytes(message);
        byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
        handler.Send(lengthBytes);

        // Send the actual data to the server
        handler.Send(messageBytes);
    }
    
    private static void ReceiveAck(ISocketHandler handler, int expectedLength)
    {
        var bytesLength = handler.Receive(expectedLength);
        var query = handler.Receive(BitConverter.ToInt32(bytesLength));
        var parsed = ParseServerResponse(query);
        Console.WriteLine(parsed);
        Logger.Info(parsed);
    }
    
    private static string ParseServerResponse(byte[] data)
    {
        return Encoding.UTF8.GetString(data);
    }
}