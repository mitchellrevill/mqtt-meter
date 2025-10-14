using Server.Hubs;
using Server.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Configure RabbitMQ
var rabbitMqOptions = new RabbitMqOptions();
builder.Configuration.GetSection("RabbitMQ").Bind(rabbitMqOptions);
builder.Services.AddSingleton(rabbitMqOptions);
builder.Services.AddSingleton<RabbitMqConnection>();

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

// TODO: Add service registrations here (ConnectionRegistry, BillCalculator, BillingWorker, etc.)

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseRouting();
app.MapControllers();
app.MapHub<BillingHub>("/hub/billing");

app.Run();
