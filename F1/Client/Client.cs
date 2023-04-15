using System.Net;
using System.Net.Sockets;
using System.Text;
using F1;
using log4net;
using ProtocolHelper;

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
            Logger.Error("Exception: {0}", e);
            client.Close();
        }
    }
}