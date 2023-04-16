using System.Net;
using System.Net.Sockets;
using System.Text;
using F1;
using log4net;
using Microsoft.VisualBasic;
using ProtocolHelper;
using ProtocolHelper.Communication;
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
            
            Console.Write($"Connected to server");
            Logger.Info($"Connected to server on {serverAddress} port {serverPort}");
            
            var message = "";
            
            ProtocolData testData = new ProtocolData
            {
                Header = "REQ",
                Operation = 1,
                Query = new QueryData
                {
                    Fields = new Dictionary<string, string>
                    {
                        {"user", "pepe123"},
                        {"PW", "123456"}
                    }
                }
            };
            
            while (!message.Equals("exit"))
            {
                ISocketHandler handler = new SocketHandler(client);
                
                message = Console.ReadLine();
                SendPacket(handler, testData.Header);
                SendPacket(handler, testData.Operation.ToString());
                SendPacket(handler, QueryDataSerializer.Serialize(testData.Query));
                
                // Receive and print the server response
                ReceiveAck(handler, Constants.FixedLength);
                ReceiveAck(handler, Constants.FixedLength);
                ReceiveAck(handler, Constants.FixedLength);
                ReceiveAck(handler, Constants.FixedLength);
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
        Console.WriteLine("Received: " + ParseServerResponse(query));
    }
    
    private static string ParseServerResponse(byte[] data)
    {
        return Encoding.UTF8.GetString(data);
    }
}