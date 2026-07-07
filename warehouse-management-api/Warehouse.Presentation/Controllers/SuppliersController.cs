using Microsoft.AspNetCore.Mvc;
using warehouse_management_api.Contracts;

namespace warehouse_management_api.Controllers;

[ApiController]
[Route("api/suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }
    //get all suppliers
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_supplierService.GetAll());
    }
    //get supplier by id
    [HttpGet("{id}")]
    public IActionResult GetById([FromRoute] string id)
    {
        var supplier = _supplierService.GetById(id);

        if (supplier is null)
        {
            return NotFound($"Supplier with id '{id}' was not found.");
        }

        return Ok(supplier);
    }
    //create supplier
    [HttpPost]
    public IActionResult Create([FromBody] CreateSupplierRequest request)
    {
        var supplier = _supplierService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, supplier);
    }
    //delete supplier
    [HttpDelete("{id}")]
    public IActionResult Deactivate([FromRoute] string id)
    {
        var deactivated = _supplierService.Deactivate(id);

        if (!deactivated)
        {
            return NotFound($"Supplier with id '{id}' was not found.");
        }

        return NoContent();
    }
}