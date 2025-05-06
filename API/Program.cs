using API.Services;
using API.Swagger;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.CarRepo;
using DataAccess.Repositories;
using DataAccess.Repositories.TestRepo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotenv.net;
using Microsoft.AspNetCore.Http.Features;
using DataAccess.Repositories.StationRepo;
using API.Hubs;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.OpenApi.Models;




var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
//builder.Services.AddSingleton<SqlDependencyService>();
//builder.Services.AddHostedService<SqlDependencyService>();

//=================================
// Cloudinary configuration
DotEnv.Load();
Cloudinary cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
cloudinary.Api.Secure = true;
builder.Services.AddSingleton(cloudinary);
// Cho phép upload file lớn (tối đa 50MB)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024;
});
//=======================================
//Redis conection
var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD");
if (string.IsNullOrEmpty(redisConnectionString))
{
    throw new ArgumentException("Redis connection string is missing");
}

var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
configurationOptions.User = "default";
configurationOptions.Password = redisPassword; 
configurationOptions.AbortOnConnectFail = false;
configurationOptions.ConnectTimeout = 15000;
configurationOptions.SyncTimeout = 15000;

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connection = ConnectionMultiplexer.Connect(configurationOptions);

    if (!connection.IsConnected)
    {
        throw new InvalidOperationException("Failed to connect to Redis");
    }

    return connection;
});
//=======================================
// Add services to the container.
builder.Services.AddDbContext<WccsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("value")), ServiceLifetime.Scoped);

builder.Services.AddScoped<CarService>();
builder.Services.AddScoped<PaymentService>();

builder.Services.AddScoped<TestService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IOtpServices, OtpServices>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<OtpServices>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryWrapper>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<FeedbackService>();
builder.Services.AddScoped<DocumentReviewService>();
builder.Services.AddScoped<ITest, Test>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICccdRepository, CccdRepository>();
builder.Services.AddScoped<IChargingStationRepository, ChargingStationRepository>();
builder.Services.AddScoped<ChargingStationService>();
builder.Services.AddScoped<IChargingPointRepository, ChargingPointRepository>();
builder.Services.AddScoped<IMyCars, MyCarsRepo>();
builder.Services.AddScoped<IDriverLicenseRepository, DriverLicenseRepository>();
builder.Services.AddScoped<IBalancement, BalanceRepo>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddHostedService<SqlDependencyService>();
builder.Services.AddScoped<IDocumentReviewRepository, DocumentReviewRepository>();

//=======================================
// JWt configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
var key = Encoding.UTF8.GetBytes(secret!);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Kiểm tra Issuer
            ValidateAudience = true, // Kiểm tra Audience
            ValidateLifetime = true, // Kiểm tra thời hạn của token
            ValidateIssuerSigningKey = true, // Kiểm tra chữ ký và private key
            ValidIssuer = jwtSettings["Issuer"], // Cấu hình Issuer hợp lệ
            ValidAudience = jwtSettings["Audience"], // Cấu hình Audience hợp lệ
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });
// Thêm dịch vụ CORS cho phép tất cả các nguồn truy cập
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAllOrigins",
//        policy =>
//        {
//            policy.WithOrigins("https://localhost:5216", "http://localhost:5216")
//                  .AllowAnyHeader()   // Cho phép bất kỳ header nào
//                  .AllowAnyMethod() // Cho phép bất kỳ method nào (GET, POST, PUT, DELETE, v.v.)
//                  .AllowCredentials();
//        });
//});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Operator", policy => policy.RequireRole("Operator"));
    options.AddPolicy("Driver", policy => policy.RequireRole("Driver"));
    options.AddPolicy("AdminOrOperator", policy => policy.RequireRole("Admin", "Operator"));
    options.AddPolicy("DriverOrOperator", policy => policy.RequireRole("Driver", "Operator"));
    options.AddPolicy("StationOwnerOrOperator", policy => policy.RequireRole("Station Owner", "Operator"));

});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

//Ratelimit
builder.Services.AddRateLimiter(options =>
{
    // Tạo policy giới hạn 10 requests/phút cho mỗi IP
    options.AddFixedWindowLimiter("Login", policy =>
    {
        policy.PermitLimit = 5;
        policy.Window = TimeSpan.FromMinutes(30);
        policy.QueueLimit = 0;
    });  
    options.AddFixedWindowLimiter("Register", policy =>
    {
        policy.PermitLimit = 5;
        policy.Window = TimeSpan.FromHours(1);
        policy.QueueLimit = 0;
    });

    // Xử lý response khi vượt limit
    options.OnRejected = async (context, _) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.Headers["Retry-After"] = "60";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            Title = "Quá nhiều request",
            detail = "Bạn đã vượt quá giới hạn request. Vui lòng thử lại sau",
            status = 429
        });
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // 1) Core metadata
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1",
        Description = "Your API description here"
    });

    // 2) JWT Bearer definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.\n\n"
                    + "Enter: **Bearer <your JWT token>**",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // 3) Apply globally (all endpoints)
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                },
                Scheme = "Bearer",
                Name   = "Authorization",
                In     = ParameterLocation.Header
            },
            Array.Empty<string>()
        }
    });
});

//=======================================
var app = builder.Build();

//var sqlDependencyService = app.Services.GetRequiredService<SqlDependencyService>();
//sqlDependencyService.StartListening();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAllOrigins");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseRateLimiter();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<RealTimeHub>("/realtimeHub"); // Đảm bảo endpoint đúng
    endpoints.MapControllers();
});

app.MapControllers();
app.Run();
