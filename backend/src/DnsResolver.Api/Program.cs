using DnsResolver.Api.Middleware;
using DnsResolver.Application.Commands.ResolveDns;
using DnsResolver.Application.Commands.CompareDns;
using DnsResolver.Application.Commands.DdnsTask;
using DnsResolver.Application.Queries.GetIsps;
using DnsResolver.Application.Queries.GetDdnsTasks;
using DnsResolver.Application.Services;
using DnsResolver.Domain.Aggregates.IspProvider;
using DnsResolver.Domain.Aggregates.DdnsTask;
using DnsResolver.Domain.Services;
using DnsResolver.Infrastructure.Configuration;
using DnsResolver.Infrastructure.DnsClient;
using DnsResolver.Infrastructure.Repositories;
using DnsResolver.Infrastructure.DnsProviders;
using DnsResolver.Infrastructure.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "DNS Resolver API",
        Version = "v1",
        Description = "多运营商域名解析管理面板 API - 支持查询和对比不同运营商的 DNS 解析结果，集成 23 个域名服务商",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "DNS Resolver",
            Url = new Uri("https://github.com/yourusername/dns-resolver")
        }
    });

    // Enable XML comments for better API documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// HttpClient for DDNS service
builder.Services.AddHttpClient();

// Configuration
builder.Services.Configure<DnsSettings>(builder.Configuration.GetSection("DnsSettings"));

// Domain & Infrastructure services
builder.Services.AddSingleton<IIspProviderRepository, InMemoryIspProviderRepository>();
builder.Services.AddSingleton<IDdnsTaskRepository, InMemoryDdnsTaskRepository>();
builder.Services.AddSingleton<IDnsResolutionService, DnsClientAdapter>();

// DNS Providers
builder.Services.AddDnsProviders();

// Application services
builder.Services.AddScoped<IDdnsService, DdnsService>();

// Application handlers
builder.Services.AddScoped<ResolveDnsCommandHandler>();
builder.Services.AddScoped<CompareDnsCommandHandler>();
builder.Services.AddScoped<GetIspsQueryHandler>();

// DDNS Task handlers
builder.Services.AddScoped<CreateDdnsTaskCommandHandler>();
builder.Services.AddScoped<UpdateDdnsTaskCommandHandler>();
builder.Services.AddScoped<DeleteDdnsTaskCommandHandler>();
builder.Services.AddScoped<GetDdnsTasksQueryHandler>();

// Background services
builder.Services.AddHostedService<DdnsSchedulerService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173"])
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Serve static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Fallback to index.html for SPA routing
app.MapFallbackToFile("index.html");

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
