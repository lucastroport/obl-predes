using System.Text;
using F1.Constants;
using ProtocolHelper.Communication.Models;

namespace ProtocolHelper.Communication;

public class ProtocolProcessor
{
    private INetworkHandler _handler;
    
    public ProtocolProcessor(INetworkHandler handler)
    {
        _handler = handler;
    }

    public async Task<ProtocolData> Process()
    {
        // Receive the header from the client
        var headerLength = ProtocolData.HeaderMaxLength;
        _handler.Receive(headerLength+1);
        var header = await _handler.Receive(headerLength);
        var bytesRead = header.Length;

        // Receive the operation from the client
        var operationLength = ProtocolData.OperationMaxLength;
        _handler.Receive(bytesRead, bytesRead);
        var operation = await _handler.Receive(operationLength);
        var parsedOperation = ParseResponse(operation);
        bytesRead += operation.Length;
                    
        // Receive the query length from the client
        var queryLengthValue = ProtocolData.QueryMaxLength;
        _handler.Receive(bytesRead, bytesRead);
        var queryLength = await _handler.Receive(queryLengthValue);
        var rawQueryLength = ParseResponse(queryLength);
        var parsedQueryLengthValue = int.Parse(rawQueryLength);
        bytesRead += queryLength.Length;
                    
        // Receive the query itself
        _handler.Receive(bytesRead, bytesRead);
        var queryRaw = await _handler.Receive(parsedQueryLengthValue);
        var parsedQuery = ParseResponse(queryRaw);
        var query = QueryDataSerializer.Deserialize(parsedQuery);

        return new ProtocolData(
            false,
            parsedOperation,
            query
        );
    }
    
    private string ParseResponse(byte[] data)
    {
        return Encoding.UTF8.GetString(data);
    }
    
    public void Send(string ack)
    {
        byte[] responseBytes = Encoding.ASCII.GetBytes(ack);
        byte[] responseLengthBytes = BitConverter.GetBytes(responseBytes.Length);
        _handler.Send(responseLengthBytes);
        _handler.Send(responseBytes);
    }
}