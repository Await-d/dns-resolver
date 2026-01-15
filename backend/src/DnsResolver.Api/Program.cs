using DnsResolver.Api.Middleware;
using DnsResolver.Application.Commands.ResolveDns;
using DnsResolver.Application.Commands.CompareDns;
using DnsResolver.Application.Commands.DdnsTask;
using DnsResolver.Application.Commands.Auth;
using DnsResolver.Application.Queries.GetIsps;
using DnsResolver.Application.Queries.GetDdnsTasks;
using DnsResolver.Application.Services;
using DnsResolver.Domain.Aggregates.IspProvider;
using DnsResolver.Domain.Aggregates.DdnsTask;
using DnsResolver.Domain.Aggregates.User;
using DnsResolver.Domain.Services;
using DnsResolver.Infrastructure.Configuration;
using DnsResolver.Infrastructure.DnsClient;
using DnsResolver.Infrastructure.Repositories;
using DnsResolver.Infrastructure.DnsProviders;
using DnsResolver.Infrastructure.BackgroundServices;
using DnsResolver.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DNS Resolver API",
        Version = "v1",
        Description = "多运营商域名解析管理面板 API - 支持查询和对比不同运营商的 DNS 解析结果，集成 23 个域名服务商",
        Contact = new OpenApiContact
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

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// HttpClient for DDNS service
builder.Services.AddHttpClient();

// JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "DnsResolver_Default_Secret_Key_For_Development_Only_32bytes!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DnsResolver";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "DnsResolver";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configuration
builder.Services.Configure<DnsSettings>(builder.Configuration.GetSection("DnsSettings"));

// Domain & Infrastructure services
builder.Services.AddSingleton<IIspProviderRepository, InMemoryIspProviderRepository>();
builder.Services.AddSingleton<IDdnsTaskRepository, InMemoryDdnsTaskRepository>();
builder.Services.AddSingleton<IDnsResolutionService, DnsClientAdapter>();

// User & Authentication services
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

// Auth handlers
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<ChangePasswordCommandHandler>();

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

app.UseAuthentication();
app.UseAuthorization();

// Serve static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Fallback to index.html for SPA routing
app.MapFallbackToFile("index.html");

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
