using F1.Domain.Model;
using Grpc.Core;
using Server;
using Server.AdminServices;

namespace GrpcServer.Services;

public class PartsService : Parts.PartsBase
{
    private PartsManager _partsManager;

    public PartsService()
    {
        _partsManager = new PartsManager();
    }
    public override async Task<AddPartResponse> AddPart(AddPartRequest request, ServerCallContext context)
    {
        var result = await _partsManager.AddPart(new Part(request.Name, request.Supplier, request.Brand));
        var addPartResponse = new AddPartResponse
        {
            AddPartResult = result.Message
        };
        return addPartResponse;
    }

    public override async Task<ModifyPartResponse> ModifyPart(ModifyPartRequest request, ServerCallContext context)
    {
        var part = new Part(request.Name, request.Supplier, request.Brand);
        part.Id = $"{request.Id}";
        var result = await _partsManager.ModifyPart(part);
        var modifyPartResponse = new ModifyPartResponse
        {
            ModifyPartResult = result.Message
        };
        return modifyPartResponse;
    }

    public override async Task<DeletePartResponse> DeletePart(DeletePartRequest request, ServerCallContext context)
    {
        var result = await _partsManager.DeletePart(request.Id);
        var deletePartResponse = new DeletePartResponse
        {
            DeletePartResult = result.Message
        };
        return deletePartResponse;
    }

    public override async Task<DeletePartFileResponse> DeletePartFile(DeletePartFileRequest request, ServerCallContext context)
    {
        var result = await _partsManager.DeletePartFile(request.PartId);
        var deletePartFileResponse = new DeletePartFileResponse()
        {
            DeletePartFileResult = result.Message
        };
        return deletePartFileResponse;
    }
}