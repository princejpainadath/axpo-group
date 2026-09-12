using AxpoGroup.PowerPosition.Application.DTOs;
using AxpoGroup.PowerPosition.Application.Interfaces;
using AxpoGroup.PowerPosition.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace AxpoGroup.PowerPosition.Infrastructure.Exporters
{
    public class CsvReportWriter : IReportWriter
    {
        private readonly ILogger<CsvReportWriter> _logger;
        private readonly CsvExportOptions _options;

        public CsvReportWriter(
            ILogger<CsvReportWriter> logger,
            IOptions<CsvExportOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        /// <summary>
        /// Exports a collection of power position records to a CSV file.
        /// The file name and output directory are determined by the configured options.
        /// Each record is written with its local time and volume.
        /// </summary>
        /// <param name="positions">The collection of power position data to be exported.</param>
        /// <param name="cancellationToken">A token used to cancel the export operation while writing records.</param>
        /// <returns></returns>
        public void Export(
            IEnumerable<PowerPositionDto> positions, 
            CancellationToken cancellationToken)
        {
            var timestamp = DateTime.Now.ToString(_options.TimestampFormat, CultureInfo.InvariantCulture);
            var fileName = _options.FileNamePattern.Replace("{timestamp}", timestamp);
            var fullPath = Path.Combine(_options.OutputDirectory, fileName);

            try
            {
                Directory.CreateDirectory(_options.OutputDirectory);

                using (var writer = new StreamWriter(fullPath))
                {
                    writer.WriteLine("Local Time,Volume");

                    foreach (var position in positions)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        writer.WriteLine($"{position.LocalTime},{position.Volume}");
                    }
                }
                _logger.LogInformation("CSV report successfully written: {fileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{function} failed to write CSV report: {fileName}.", nameof(Export), fileName);
                throw;
            }
        }
    }
}
