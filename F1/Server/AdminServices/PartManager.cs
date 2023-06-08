using F1.Domain.Model;
using F1.Domain.Repository;
using Server.Grpc;

namespace Server.AdminServices;

public class PartsManager
{
    private IPartRepository _partRepository;

    public PartsManager()
    {
        _partRepository = PartRepository.Instance;
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

            return Task.FromResult(response);
        }
        catch (ArgumentException ex)
        {
            var response = new PartServiceResponse
            {
                Message = ex.Message
            };

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

            return Task.FromResult(errorResponse);
        }

        part.Name = request.Name;
        part.Supplier = request.Supplier;
        part.Brand = request.Brand;

        var response = new PartServiceResponse
        {
            Message = $"Part '{part.Name}' modified successfully."
        };

        return Task.FromResult(response);
    }

    public Task<PartServiceResponse> DeletePart(int id)
    {
        var part = _partRepository.QueryById($"{id}");
        if (part == null)
        {
            var erroResponse = new PartServiceResponse
            {
                Message = $"Part with ID '{id}' not found."
            };

            return Task.FromResult(erroResponse);
        }

        _partRepository.RemovePart(part);

        var response = new PartServiceResponse
        {
            Message = $"Part '{part.Name}' deleted successfully."
        };

        return Task.FromResult(response);
    }
}
