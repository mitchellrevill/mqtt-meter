using Infrastructure;
using Application;
using Infrastructure.DatabaseContext;
using Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddApplicationServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

using(var serviceScope = app.Services.CreateScope())
using(var DbContext = serviceScope.ServiceProvider.GetRequiredService<MqttDbContext>())
{
    DbContext.Database.EnsureCreated();

    GeneratorHelper.SeedData(DbContext);
}

app.MapControllers();

app.Run();
