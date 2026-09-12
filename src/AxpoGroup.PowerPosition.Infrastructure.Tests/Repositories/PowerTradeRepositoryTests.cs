using AutoMapper;
using Axpo;
using AxpoGroup.PowerPosition.Application.DTOs;
using AxpoGroup.PowerPosition.Infrastructure.Repositories;
using AxpoGroup.PowerPosition.Infrastructure.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace AxpoGroup.PowerPosition.Infrastructure.Tests.Repositories
{
    public class PowerTradeRepositoryTests
    {
        private readonly Mock<ILogger<PowerTradeRepository>> _loggerMock;
        private readonly Mock<IPowerService> _powerServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly PowerTradeRepository _repository;

        public PowerTradeRepositoryTests()
        {
            _loggerMock = new Mock<ILogger<PowerTradeRepository>>();
            _powerServiceMock = new Mock<IPowerService>();
            _mapperMock = new Mock<IMapper>();

            _repository = new PowerTradeRepository(
                _loggerMock.Object,
                _powerServiceMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task GetTradesAsync_ValidRequest_ReturnsMappedDtos()
        {
            // Arrange
            var date = DateTime.Today;
            var trades = TestDataHelper.GetValidTrades(date);

            _powerServiceMock.Setup(s => s.GetTradesAsync(date)).ReturnsAsync(trades);

            _mapperMock.Setup(m => m.Map<IEnumerable<PowerTradeDto>>(trades))
                .Returns(new List<PowerTradeDto> { new PowerTradeDto { Date = date, Periods = new List<PowerPeriodDto>() } });

            // Act
            var result = await _repository.GetTradesAsync(date);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(date, result.First().Date);
        }

        [Fact]
        public async Task GetTradesAsync_NoTrades_ReturnsEmptyCollection()
        {
            // Arrange
            var date = DateTime.Today;

            _powerServiceMock.Setup(s => s.GetTradesAsync(date)).ReturnsAsync(new List<PowerTrade>());

            _mapperMock.Setup(m => m.Map<IEnumerable<PowerTradeDto>>(It.IsAny<IEnumerable<PowerTrade>>()))
                .Returns(new List<PowerTradeDto>());

            // Act
            var result = await _repository.GetTradesAsync(date);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTradesAsync_ServiceThrows_LogsErrorAndRethrows()
        {
            // Arrange
            var date = DateTime.Today;

            _powerServiceMock.Setup(s => s.GetTradesAsync(date))
                .ThrowsAsync(new Exception("Service failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _repository.GetTradesAsync(date));

            _loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(lvl => lvl == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}
