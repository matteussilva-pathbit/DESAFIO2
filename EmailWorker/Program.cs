using EmailWorker.Configuration;
using EmailWorker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Bind do appsettings.*.json
builder.Services.Configure<RabbitSettings>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

// Worker em background
builder.Services.AddHostedService<EmailConsumerService>();

var app = builder.Build();
app.Run();
