using Microsoft.OpenApi;
using warehouse_management_api.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();          
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddSwaggerGen(options =>
{
    options.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = JsonSchemaType.String,
        Format = "binary"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();                       
app.MapGet("/", () => "Hello World!");

app.Run();