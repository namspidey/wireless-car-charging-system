using API.Services;
using DataAccess.Interfaces;
using DataAccess.Models;

using DataAccess.Repositories.CarRepo;

using DataAccess.Repositories;

using DataAccess.Repositories.TestRepo;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<WccsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("value")));


builder.Services.AddScoped<ITest, Test>(); 
builder.Services.AddScoped<TestService>();
builder.Services.AddScoped<IChargingStationRepository, ChargingStationRepository>();
builder.Services.AddScoped<ChargingStationService>();
builder.Services.AddScoped<IChargingPointRepository, ChargingPointRepository>();

builder.Services.AddScoped<IMyCars, MyCarsRepo>(); 
builder.Services.AddScoped<CarService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()    // Cho phép mọi nguồn
                        .AllowAnyMethod()    // Cho phép mọi phương thức (GET, POST, PUT, DELETE,...)
                        .AllowAnyHeader());  // Cho phép mọi header
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
