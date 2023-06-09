using F1.Domain.Model;
using F1.Domain.Repository;
using Server.Grpc;
using Server.Logging;

namespace Server.AdminServices;

public class PartsManager
{
    private IPartRepository _partRepository;
    private RabbitMqLogger _rabbitMqLogger;

    public PartsManager()
    {
        _partRepository = PartRepository.Instance;
        _rabbitMqLogger = new RabbitMqLogger(
            LoggingConfigValues.QueueHost, 
            LoggingConfigValues.QueueUsername,
            LoggingConfigValues.QueuePassword,
            LoggingConfigValues.ExchangeName);
    }

    public Task<PartServiceResponse> AddPart(Part request)
    {
        try
        {
            
            var part = new Part(request.Name, request.Supplier, request.Brand);

            _partRepository.AddPart(part);

            var response = new PartServiceResponse
            {
                Message = $"Part '{part.Name}' added successfully."
            };
            _rabbitMqLogger.LogPartInfo($"(ADMIN SERVER) {response.Message}");

            return Task.FromResult(response);
        }
        catch (ArgumentException ex)
        {
            var response = new PartServiceResponse
            {
                Message = ex.Message
            };
            _rabbitMqLogger.LogError($"(ADMIN SERVER) {response.Message}");
            return Task.FromResult(response);
        }
    }

    public Task<PartServiceResponse> ModifyPart(Part request)
    {
        var part = _partRepository.QueryById(request.Id);
        if (part == null)
        {
            var errorResponse = new PartServiceResponse
            {
                Message = $"Part with ID '{request.Id}' not found."
            };
            _rabbitMqLogger.LogPartInfo($"(ADMIN SERVER) {errorResponse.Message}");
            return Task.FromResult(errorResponse);
        }

        part.Name = request.Name;
        part.Supplier = request.Supplier;
        part.Brand = request.Brand;

        var response = new PartServiceResponse
        {
            Message = $"Part '{part.Name}' modified successfully."
        };
        
        _rabbitMqLogger.LogPartInfo($"(ADMIN SERVER) {response.Message}");
        return Task.FromResult(response);
    }

    public Task<PartServiceResponse> DeletePart(int id)
    {
        var part = _partRepository.QueryById($"{id}");
        if (part == null)
        {
            var errorResponse = new PartServiceResponse
            {
                Message = $"Part with ID '{id}' not found."
            };

            _rabbitMqLogger.LogPartInfo($"(ADMIN SERVER) {errorResponse.Message}");
            return Task.FromResult(errorResponse);
        }

        _partRepository.RemovePart(part);

        var response = new PartServiceResponse
        {
            Message = $"Part '{part.Name}' deleted successfully."
        };
        _rabbitMqLogger.LogPartInfo($"(ADMIN SERVER) {response.Message}");
        return Task.FromResult(response);
    }
    
    public Task<PartServiceResponse> DeletePartFile(int id)
    {
        var part = _partRepository.QueryById($"{id}");
        if (part == null)
        {
            var errorResponse = new PartServiceResponse
            {
                Message = $"Part with ID '{id}' not found."
            };
            _rabbitMqLogger.LogPartInfo($"(ADMIN SERVER) {errorResponse.Message}");
            return Task.FromResult(errorResponse);
        }
        if (part.PhotoUrl == null)
        {
            _rabbitMqLogger.LogPartInfo($"(ADMIN SERVER) Part with ID '{id}' has no associated PhotoUrl.");
            return Task.FromResult(new PartServiceResponse
            {
                Message = $"Part with ID '{id}' has no associated PhotoUrl."
            });    
        }
        try
        {
            var path = part.PhotoUrl;
            File.Delete(part.PhotoUrl);
            part.PhotoUrl = null;
            var response = new PartServiceResponse
            {
                Message = $"Part file '{path}' deleted successfully."
            };
            _rabbitMqLogger.LogPartInfo($"(ADMIN SERVER) Part file '{path}' deleted successfully.");
            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            _rabbitMqLogger.LogError($"(ADMIN SERVER) Error deleting part file '{part.PhotoUrl}': {ex.Message}");
            return Task.FromResult(new PartServiceResponse
            {
                Message = $"Error deleting part file '{part.PhotoUrl}': {ex.Message}"
            });
        }
    }
}
