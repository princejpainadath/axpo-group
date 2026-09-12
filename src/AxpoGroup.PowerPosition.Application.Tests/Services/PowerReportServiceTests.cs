using AutoMapper;
using AxpoGroup.PowerPosition.Application.DTOs;
using AxpoGroup.PowerPosition.Application.Interfaces;
using AxpoGroup.PowerPosition.Application.Services;
using AxpoGroup.PowerPosition.Application.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace AxpoGroup.PowerPosition.Application.Tests.Services
{
    public class PowerReportServiceTests
    {
        private readonly Mock<ILogger<PowerReportService>> _loggerMock;
        private readonly Mock<IPowerTradeRepository> _repositoryMock;
        private readonly Mock<IReportWriter> _reportWriterMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly PowerReportService _service;

        public PowerReportServiceTests()
        {
            _loggerMock = new Mock<ILogger<PowerReportService>>();
            _repositoryMock = new Mock<IPowerTradeRepository>();
            _reportWriterMock = new Mock<IReportWriter>();
            _mapperMock = new Mock<IMapper>();

            _service = new PowerReportService(
                _loggerMock.Object,
                _repositoryMock.Object,
                _reportWriterMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task ProcessTradesAsync_WithValidTrades_AggregatesAndExportsSuccessfully()
        {
            // Arrange
            var trades = TestDataHelper.GetValidTrades();

            _repositoryMock.Setup(r => r.GetTradesAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(trades);

            _mapperMock.Setup(m => m.Map<IEnumerable<PowerPositionDto>>(It.IsAny<IEnumerable<Domain.Entities.PowerPosition>>()))
                .Returns<IEnumerable<Domain.Entities.PowerPosition>>(positions =>
                positions.Select(p => new PowerPositionDto { LocalTime = p.LocalTime, Volume = p.Volume }));

            // Act
            await _service.ProcessTradesAsync(DateTime.Today, CancellationToken.None);

            // Assert
            _reportWriterMock.Verify(r => r.Export(It.Is<IEnumerable<PowerPositionDto>>(p =>
                p.Any(x => x.LocalTime == "23:00" && x.Volume == 150) &&
                p.Any(x => x.LocalTime == "00:00" && x.Volume == 180)
            ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessTradesAsync_NoTrades_LogsWarningAndSkipsExport()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetTradesAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(Enumerable.Empty<PowerTradeDto>());

            // Act
            await _service.ProcessTradesAsync(DateTime.Today, CancellationToken.None);

            // Assert
            _reportWriterMock.Verify(r => r.Export(It.IsAny<IEnumerable<PowerPositionDto>>(), It.IsAny<CancellationToken>()), Times.Never);
            _loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(lvl => lvl == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task ProcessTradesAsync_RepositoryThrows_LogsErrorAndRethrowsException()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetTradesAsync(It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.ProcessTradesAsync(DateTime.Today, CancellationToken.None));

            _loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(lvl => lvl == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}
