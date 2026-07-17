using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Exceptions;
using warehouse_management_api.Metadata;

namespace warehouse_management_api.Controllers;

[ApiController]
[Route("api/metadata")]
public class MetadataController(ValidationMetadataProvider metadataProvider) : ControllerBase
{
    [HttpGet("validation/{dtoName}")]
    public ActionResult<DtoValidationMetadata> GetValidationMetadata([FromRoute] string dtoName)
    {
        var metadata = metadataProvider.GetMetadata(dtoName);

        if (metadata is null)
        {
            throw new NotFoundException(
                $"No known DTO named '{dtoName}'. Available: {string.Join(", ", ValidationMetadataProvider.AllowedDtoNames)}");
        }

        return Ok(metadata);
    }
}