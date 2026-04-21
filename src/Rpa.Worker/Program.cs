using Rpa.Infrastructure;
using Rpa.Worker.Configuration;
using Rpa.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<WorkerOptions>(
    builder.Configuration.GetSection(WorkerOptions.SectionName));

builder.Services.AddInfrastructure();
builder.Services.AddHostedService<ScraperWorker>();

var host = builder.Build();
host.Run();
