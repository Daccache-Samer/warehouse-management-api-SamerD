using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Warehouse.Application.Products.Commands.CreateProduct;
using Warehouse.DomainWarehouse.Domain.Products;
using Warehouse.DomainWarehouse.Domain.Suppliers;
using Warehouse.Infrastructure.Persistence.InMemory;
using Warehouse.Infrastructure.Storage;
using warehouse_management_api.Middleware;
using Warehouse.Infrastructure.Models;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<WarehouseDbFirstContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("Default Connection")));
builder.Services.AddSwaggerGen(options =>
{
    options.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = JsonSchemaType.String,
        Format = "binary"
    });
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));

builder.Services.AddSingleton<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<ISupplierRepository, SupplierRepository>();

builder.Services.AddSingleton<IFileStorage>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    return new LocalFileStorage(env.WebRootPath);
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();