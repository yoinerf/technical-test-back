using Amazon.DynamoDBv2;
using Amazon.SimpleEmail;
using Amazon.SimpleNotificationService;
using InvestmentFunds.API.Middleware;
using InvestmentFunds.Application.Interfaces;
using InvestmentFunds.Application.Services;
using InvestmentFunds.Domain.Interfaces;
using InvestmentFunds.Infrastructure.Repositories;
using InvestmentFunds.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Agregar servicios al contenedor
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no configurado");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configurar AWS Services
var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddAWSService<IAmazonSimpleEmailService>();
builder.Services.AddAWSService<IAmazonSimpleNotificationService>();

// Configurar DynamoDB Settings
builder.Services.Configure<InvestmentFunds.Infrastructure.Configuration.DynamoDBSettings>(
    builder.Configuration.GetSection("DynamoDB"));

// Registrar repositorios
builder.Services.AddScoped<ICustomerRepository, DynamoDBCustomerRepository>();
builder.Services.AddScoped<IFundRepository, DynamoDBFundRepository>();
builder.Services.AddScoped<ITransactionRepository, DynamoDBTransactionRepository>();
builder.Services.AddScoped<ISubscriptionRepository, DynamoDBSubscriptionRepository>();

// Registrar servicios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Middleware de manejo de excepciones global
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
