using System.Net.Sockets;
using F1.Constants;
using ProtocolHelper.Communication.Models;
using ProtocolHelper.FileHandler;

namespace ProtocolHelper.Communication;

public class FileCommsHandler
{
    private readonly ConversionHandler _conversionHandler;
    private readonly FileHandler.FileHandler _fileHandler;
    private readonly FileStreamHandler _fileStreamHandler;
    private readonly ProtocolProcessor _protocolProcessor;
    private readonly ISocketHandler _socketHandler;

    public FileCommsHandler(Socket socket)
    {
        _conversionHandler = new ConversionHandler();
        _fileHandler = new FileHandler.FileHandler();
        _fileStreamHandler = new FileStreamHandler();
        _socketHandler = new SocketHandler(socket);
        _protocolProcessor = new ProtocolProcessor(_socketHandler);
    }

    public void SendFile(string path, ProtocolData protocolData)
    {
        if (_fileHandler.FileExists(path))
        {
            var fileName = _fileHandler.GetFileName(path);
            // _socketHandler.Send(_conversionHandler.ConvertIntToBytes(fileName.Length));
            // _socketHandler.Send(_conversionHandler.ConvertStringToBytes(fileName));

            long fileSize = _fileHandler.GetFileSize(path);
            var convertedFileSize = _conversionHandler.ConvertLongToBytes(fileSize);
            // _socketHandler.Send(convertedFileSize);
            var query = new QueryData
            {
                Fields = new Dictionary<string, string>
                {
                    {ConstantKeys.FileNameKey, fileName},
                    {ConstantKeys.FileSizeKey, $"{fileSize}"}
                }
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

    public void ReceiveFile(long fileSize, string filename)
    {
        // var protocolData = _protocolProcessor.Process();
        // protocolData.Query.Fields.TryGetValue(ConstantKeys.FileNameKey, out var filename);
        // int fileNameSize = _conversionHandler.ConvertBytesToInt(
        //     _socketHandler.Receive(FileUtils.FixedDataSize));
        // string fileName = _conversionHandler.ConvertBytesToString(_socketHandler.Receive(fileNameSize));
        // protocolData.Query.Fields.TryGetValue(ConstantKeys.FileSizeKey, out var fileSizeRaw);
        // long fileSize = long.Parse(fileSizeRaw);
        
        ReceiveFileWithStreams(fileSize, filename);
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

            _socketHandler.Send(data);
            currentPart++;
        }
    }

    private void ReceiveFileWithStreams(long fileSize, string fileName)
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
                data = _socketHandler.Receive(lastPartSize);
                offset += lastPartSize;
            }
            else
            {
                data = _socketHandler.Receive(FileUtils.MaxPacketSize);
                offset += FileUtils.MaxPacketSize;
            }
            _fileStreamHandler.Write(fileName, data);
            currentPart++;
        }
    }
}