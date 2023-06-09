using Microsoft.AspNetCore.Mvc;
using Server;

namespace AdminServer.Controllers;

[ApiController]
[Route("api/parts")]
public class PartsController : ControllerBase
{
    private readonly Parts.PartsClient _grpcClient;

    public PartsController(Parts.PartsClient grpcClient)
    {
        AppContext.SetSwitch(
            "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        _grpcClient = grpcClient;
    }

    [HttpPost]
    public async Task<IActionResult> AddPart(AddPartRequest request)
    {
        var response = await _grpcClient.AddPartAsync(request);

        return Ok(response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ModifyPart(int id, ModifyPartRequest partRequest)
    {
        partRequest.Id = id;
        var response = await _grpcClient.ModifyPartAsync(partRequest);

        return Ok(response);
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePart(int id)
    {
        var request = new DeletePartRequest();
        request.Id = id;
        
        var response = await _grpcClient.DeletePartAsync(request);

        return Ok(response);
    }
    
    [HttpDelete("{id:int}/file")]
    public async Task<IActionResult> DeletePartFile(int id)
    {
        var request = new DeletePartFileRequest
        {
            PartId = id
        };

        var response = await _grpcClient.DeletePartFileAsync(request);

        return Ok(response);
    }

}
