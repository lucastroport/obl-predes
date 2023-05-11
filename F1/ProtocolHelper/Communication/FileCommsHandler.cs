using System.Net.Sockets;
using F1.Constants;
using ProtocolHelper.Communication.Models;
using ProtocolHelper.FileHandler;

namespace ProtocolHelper.Communication;

public class FileCommsHandler
{
    private readonly FileHandler.FileHandler _fileHandler;
    private readonly FileStreamHandler _fileStreamHandler;
    private readonly ProtocolProcessor _protocolProcessor;
    private readonly INetworkHandler _networkHandler;

    public FileCommsHandler(TcpClient socket)
    {
        _fileHandler = new FileHandler.FileHandler();
        _fileStreamHandler = new FileStreamHandler();
        _networkHandler = new NetworkHandler(socket);
        _protocolProcessor = new ProtocolProcessor(_networkHandler);
    }

    public void SendFile(string path, ProtocolData protocolData)
    {
        if (_fileHandler.FileExists(path))
        {
            var fileName = _fileHandler.GetFileName(path);
            long fileSize = _fileHandler.GetFileSize(path);
            protocolData.Query.Fields.Add(ConstantKeys.FileNameKey, fileName);
            protocolData.Query.Fields.Add(ConstantKeys.FileSizeKey, $"{fileSize}");
            
            var query = new QueryData
            {
                Fields = protocolData.Query.Fields
            };
            protocolData.Query = query;

            var data = new ProtocolData(
                true,
                protocolData.Operation,
                query
            );
            var serializedQuery = QueryDataSerializer.Serialize(protocolData.Query);
            
            _protocolProcessor.Send(
                data.Header +
                data.Operation +
                data.QueryLength +
                serializedQuery
            );
            SendFileWithStream(fileSize, path);
        }
        else
        {
            throw new Exception("File does not exist");
        }
    }

    public async Task<string> ReceiveFile(long fileSize, string filename)
    {
        return await ReceiveFileWithStreams(fileSize, filename);
    }

    private void SendFileWithStream(long fileSize, string path)
    {
        long fileParts = FileUtils.CalculateFileParts(fileSize);
        long offset = 0;
        long currentPart = 1;

        while (fileSize > offset)
        {
            byte[] data;
            if (currentPart == fileParts)
            {
                var lastPartSize = (int)(fileSize - offset);
                data = _fileStreamHandler.Read(path, offset, lastPartSize);
                offset += lastPartSize;
            }
            else
            {
                data = _fileStreamHandler.Read(path, offset, FileUtils.MaxPacketSize);
                offset += FileUtils.MaxPacketSize;
            }

            _networkHandler.Send(data);
            currentPart++;
        }
    }

    private async Task<string> ReceiveFileWithStreams(long fileSize, string fileName)
    {
        long fileParts = FileUtils.CalculateFileParts(fileSize);
        long offset = 0;
        long currentPart = 1;
        var writePath = "";
            
        while (fileSize > offset)
        {
            byte[] data;
            if (currentPart == fileParts)
            {
                var lastPartSize = (int)(fileSize - offset);
                data = await _networkHandler.Receive(lastPartSize);
                offset += lastPartSize;
            }
            else
            {
                data = await _networkHandler.Receive(FileUtils.MaxPacketSize);
                offset += FileUtils.MaxPacketSize;
            }
            writePath = _fileStreamHandler.Write(fileName, data);
            currentPart++;
        }

        return writePath;
    }
}