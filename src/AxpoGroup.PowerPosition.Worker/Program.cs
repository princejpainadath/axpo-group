using Axpo;
using AxpoGroup.PowerPosition.Application.Interfaces;
using AxpoGroup.PowerPosition.Application.Mappers;
using AxpoGroup.PowerPosition.Application.Options;
using AxpoGroup.PowerPosition.Application.Services;
using AxpoGroup.PowerPosition.Infrastructure.Exporters;
using AxpoGroup.PowerPosition.Infrastructure.Repositories;
using AxpoGroup.PowerPosition.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Bind options
builder.Services.AddOptions<ScheduledExtractOptions>()
    .Bind(builder.Configuration.GetSection(nameof(ScheduledExtractOptions)))
    .ValidateDataAnnotations();
builder.Services.AddOptions<CsvExportOptions>()
    .Bind(builder.Configuration.GetSection(nameof(CsvExportOptions)));

// Register autoMapper
builder.Services.AddAutoMapper(cfg => { cfg.AddProfile<MappingProfile>(); });

// Register application services
builder.Services.AddScoped<IPowerReportService, PowerReportService>();

// Register infrastructure services 
builder.Services.AddScoped<IPowerService, PowerService>();
builder.Services.AddScoped<IPowerTradeRepository, PowerTradeRepository>();
builder.Services.AddScoped<IReportWriter, CsvReportWriter>();

// Register the worker as a hosted service
builder.Services.AddHostedService<ScheduledExtractWorker>();

var host = builder.Build();
host.Run();
