using AxpoGroup.PowerPosition.Application.DTOs;
using AxpoGroup.PowerPosition.Application.Options;
using AxpoGroup.PowerPosition.Infrastructure.Exporters;
using AxpoGroup.PowerPosition.Infrastructure.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AxpoGroup.PowerPosition.Infrastructure.Tests.Exporters
{
    public class CsvReportWriterTests
    {
        private readonly Mock<ILogger<CsvReportWriter>> _loggerMock;
        private readonly CsvExportOptions _options;
        private readonly CsvReportWriter _writer;

        public CsvReportWriterTests()
        {
            _loggerMock = new Mock<ILogger<CsvReportWriter>>();
            _options = new CsvExportOptions
            {
                OutputDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()),
                FileNamePattern = "PowerPosition_{timestamp}.csv",
                TimestampFormat = "yyyyMMdd_HHmm"
            };

            _writer = new CsvReportWriter(_loggerMock.Object, Options.Create(_options));
        }

        [Fact]
        public void Export_ValidPositions_WritesCsvFile()
        {
            // Arrange
            var positions = TestDataHelper.GetValidPositions();

            // Act
            _writer.Export(positions, CancellationToken.None);

            // Assert
            var files = Directory.GetFiles(_options.OutputDirectory);
            Assert.Single(files);

            var content = File.ReadAllText(files[0]);
            Assert.Contains("Local Time,Volume", content);
            Assert.Contains("23:00,150", content);
            Assert.Contains("00:00,200", content);

            _loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(lvl => lvl == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void Export_CancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            var positions = TestDataHelper.GetValidPositions();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            Assert.Throws<OperationCanceledException>(() => _writer.Export(positions, cts.Token));
        }

        [Fact]
        public void Export_WhenExceptionOccurs_LogsErrorAndRethrows()
        {
            // Arrange
            var badOptions = new CsvExportOptions
            {
                OutputDirectory = "?:\\InvalidPath", // invalid path to force exception
                FileNamePattern = "PowerPosition_{timestamp}.csv",
                TimestampFormat = "yyyyMMdd_HHmm"
            };
            var writer = new CsvReportWriter(_loggerMock.Object, Options.Create(badOptions));

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => writer.Export(new List<PowerPositionDto>(), CancellationToken.None));

            _loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(lvl => lvl == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}
