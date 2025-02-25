using API.Services;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories;
using DataAccess.Repositories.TestRepo;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<WccsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("value")));


builder.Services.AddScoped<ITest, Test>(); // Đăng ký đúng class thực thi
builder.Services.AddScoped<TestService>();
builder.Services.AddScoped<IChargingStationRepository, ChargingStationRepository>();
builder.Services.AddScoped<ChargingStationService>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
