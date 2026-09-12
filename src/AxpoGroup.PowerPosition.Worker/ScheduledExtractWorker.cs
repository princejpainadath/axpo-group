using AxpoGroup.PowerPosition.Application.Interfaces;
using AxpoGroup.PowerPosition.Application.Options;
using Microsoft.Extensions.Options;

namespace AxpoGroup.PowerPosition.Worker
{
    public class ScheduledExtractWorker : BackgroundService
    {
        private readonly ILogger<ScheduledExtractWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ScheduledExtractOptions _options;

        public ScheduledExtractWorker(
            ILogger<ScheduledExtractWorker> logger,
            IServiceProvider serviceProvider,
            IOptions<ScheduledExtractOptions> options)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        /// <summary>
        /// Executes the background worker process.
        /// Continuously runs scheduled extracts at the configured interval until cancellation is requested.
        /// </summary>
        /// <param name="stoppingToken">A cancellation token used to stop the execution loop cleanly.</param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started at (UTC): {utc}", DateTime.UtcNow);
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var powerReportService = scope.ServiceProvider.GetRequiredService<IPowerReportService>();
                    await ExecuteScheduledExtractWithRetryAsync(powerReportService, stoppingToken);
                }
                await Task.Delay(TimeSpan.FromMinutes(_options.ExtractIntervalMinutes), stoppingToken);
            }
            _logger.LogInformation("Worker stopped at (UTC): {utc}", DateTime.UtcNow);
        }

        /// <summary>
        /// Executes the scheduled extract with retry logic. Attempts to process trades up to a maximum number of retries,
        /// logging each attempt and delaying briefly between failures.
        /// </summary>
        /// <param name="powerReportService">The report service used to process trades for the scheduled extract.</param>
        /// <param name="stoppingToken">A cancellation token that signals when the operation should stop, allowing retries and delays to be aborted cleanly.</param>
        /// <returns></returns>
        private async Task ExecuteScheduledExtractWithRetryAsync(
            IPowerReportService powerReportService,
            CancellationToken stoppingToken)
        {
            int maxRetries = _options.MaxRetries;
            int attempt = 0;

            while (attempt < maxRetries && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Running extract at (UTC): {utc}", DateTime.UtcNow);
                    await powerReportService.ProcessTradesAsync(DateTime.Today, stoppingToken);
                    _logger.LogInformation("Extract succeeded at (UTC): {utc}", DateTime.UtcNow);
                    return;
                }
                catch (Exception ex)
                {
                    attempt++;
                    _logger.LogError(ex, "Extract failed on attempt {attempt} at (UTC): {utc}", attempt, DateTime.UtcNow);

                    if (attempt >= maxRetries)
                        _logger.LogCritical("Scheduled extract failed after {maxRetries} attempts at (UTC): {utc}", maxRetries, DateTime.UtcNow);
                    else
                        await Task.Delay(TimeSpan.FromSeconds(_options.RetryDelaySeconds));
                }
            }
        }
    }
}
