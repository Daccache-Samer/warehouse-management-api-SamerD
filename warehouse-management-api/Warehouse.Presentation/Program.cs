using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;
using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin;
using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using warehouse_management_api.Filters;
using warehouse_management_api.Middleware;
using Warehouse.Application.BackgroundJobs;
using Warehouse.Application.Products;
using Warehouse.Application.Products.Commands.CreateProduct;
using Warehouse.Application.Trackers;
using Warehouse.DomainWarehouse.Domain.Products;
using Warehouse.DomainWarehouse.Domain.Suppliers;
using Warehouse.Infrastructure.Firebase;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Storage;
using dotenv.net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration; 
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();
builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ActionLoggingFilter>();
        options.Filters.Add<ModelValidationFilter>();
    })
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddLocalization();
string[] supportedCultures = ["en", "fr"];
builder.Services.Configure<RequestLocalizationOptions>(options => options.SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));
builder.Services.AddAutoMapper(_ =>
    { }, typeof(ProductMappingProfile).Assembly);
builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSwaggerGen(options =>
{
    options.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = JsonSchemaType.String,
        Format = "binary"
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = 
        builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "MyApp_";
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

builder.Services.AddHealthChecks()
    .AddNpgSql(configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException())
    .AddRedis(configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException());
builder.Services.AddHealthChecksUI(opts =>
{
    opts.AddHealthCheckEndpoint("api", "/health");
}).AddInMemoryStorage();

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseInMemoryStorage());

builder.Services.AddHangfireServer();
builder.Services.AddScoped<ExpiryDateCheckJob>();
builder.Services.AddSingleton<CacheStatisticsTracker>();

Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", @"warehouse-management-api-12faf-firebase-adminsdk-fbsvc-0c8a5b4099.json");
builder.Services.AddSingleton(FirebaseApp.Create());
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Authenticated");
});
var firebaseProjectName = Environment.GetEnvironmentVariable("PROJECT_ID");
var apiKey = Environment.GetEnvironmentVariable("API_KEY");
var authDomain = Environment.GetEnvironmentVariable("AUTHDOMAIN");
builder.Services.AddSingleton(new FirebaseAuthClient(new FirebaseAuthConfig
{
    ApiKey =apiKey,
    AuthDomain = authDomain,
    Providers =
    [
        new EmailProvider(),
        new GoogleProvider()
    ]
}));
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>(); 
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Unauthenticated"; 
        options.AccessDeniedPath = "/Unauthenticated";
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = $"https://google.com{firebaseProjectName}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://google.com{firebaseProjectName}",
            ValidateAudience = true,
            ValidAudience = firebaseProjectName,
            ValidateLifetime = true
        };
    });

builder.Services.AddSession();

var app = builder.Build();

app.UseMiddleware<IdCorrelationMiddleware>();
app.UseRequestLocalization();
app.UseMiddleware<RequestTimingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.Use(async (context, next) =>
{
    var token = context.Session.GetString("token");
    
    if (!string.IsNullOrEmpty(token) && context.User.Identity?.IsAuthenticated != true)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.Authentication, token) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        
        context.User = new ClaimsPrincipal(identity);
    }
    
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
    options.ApiPath = "/health-ui-api";
});

app.UseHangfireDashboard();

var cron = app.Configuration["* * * * *"] ?? Cron.Hourly();
RecurringJob.AddOrUpdate<ExpiryDateCheckJob>(
    "expiry-check",
    job => job.ExecuteAsync(CancellationToken.None),
    cron);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapRazorPages(); 
app.MapControllers();

app.Run();