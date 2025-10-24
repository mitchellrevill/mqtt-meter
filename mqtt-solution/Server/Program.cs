using Server.Hubs;
using Server.Messaging;
using Application;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddHttpClient(); // Add HttpClient factory

// Add Application layer services
builder.Services.AddApplicationServices();

// Add Infrastructure layer services (repositories, database context)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply CORS before routing for Flutter compatibility
app.UseCors("AllowAll");

app.UseRouting();
app.MapControllers();
app.MapHub<BillingHub>("/hub/billing");

app.Run();
