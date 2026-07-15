using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using warehouse_management_api.Filters;
using Warehouse.Application.Products.Commands.CreateProduct;
using Warehouse.DomainWarehouse.Domain.Products;
using Warehouse.DomainWarehouse.Domain.Suppliers;
using Warehouse.Infrastructure.Storage;
using warehouse_management_api.Middleware;
using Warehouse.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration; 
builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ActionLoggingFilter>();
        options.Filters.Add<ModelValidationFilter>();
    })
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(cfg =>
    { }, typeof(Warehouse.Application.Products.ProductMappingProfile).Assembly);
builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContextFactory<WarehouseDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
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

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddSingleton<IFileStorage>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    return new LocalFileStorage(env.WebRootPath);
});

var app = builder.Build();

app.UseMiddleware<IdCorrelationMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();