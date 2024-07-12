using Microsoft.EntityFrameworkCore;
using Demo.Entities;
using Demo.Dtos.Key;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Demo.Services.Transaction;
using Demo.Services.User;
using Demo.Services.Stock;
using Demo.Hubs;
using Demo.Services.Request;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder =>
        {
            builder.WithOrigins("https://9000-idx-testsignalr-1716261815581.cluster-bs35cdu5w5cuaxdfch3hqqt7zm.cloudworkstations.dev", "https://localhost:7278")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
});

builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IRequestService, RequestService>();
// Thêm dịch vụ vào container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPI", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddDbContext<TrungTq50demoContext>(options =>
{
    var connection = builder.Configuration.GetConnectionString("Database");
    options.UseMySql(connection!, new MySqlServerVersion(new Version(8, 0, 27)));
});

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
var secretKey = builder.Configuration["AppSettings:SecretKey"];
if (string.IsNullOrEmpty(secretKey))
{
    Console.WriteLine("Error: SecretKey is null or empty.");
    secretKey = "defaultSecretKey"; // Replace with an appropriate default value.
}

builder.Services.AddScoped<IUserService, UserService>();
var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
            ClockSkew = TimeSpan.Zero,
        };
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
                Console.WriteLine($"Đã Nhận token");
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseCors("AllowReactApp");
app.MapHub<StockTableHub>("/stockTableHub");

app.Run();
