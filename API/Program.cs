using API.Services;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.CarRepo;
using DataAccess.Repositories.TestRepo;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<WccsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("value")));


builder.Services.AddScoped<ITest, Test>(); 
builder.Services.AddScoped<TestService>();

builder.Services.AddScoped<IMyCars, MyCarsRepo>(); 
builder.Services.AddScoped<CarService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:5216") // Allow your frontend URL
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});



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
app.UseCors("AllowSpecificOrigin");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
