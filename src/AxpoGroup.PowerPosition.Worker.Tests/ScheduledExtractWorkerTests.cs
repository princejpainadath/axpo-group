using AxpoGroup.PowerPosition.Application.Interfaces;
using AxpoGroup.PowerPosition.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AxpoGroup.PowerPosition.Worker.Tests
{
    public class ScheduledExtractWorkerTests
    {
        private readonly Mock<ILogger<ScheduledExtractWorker>> _loggerMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly ScheduledExtractOptions _options;
        private readonly ScheduledExtractWorker _worker;

        public ScheduledExtractWorkerTests()
        {
            _loggerMock = new Mock<ILogger<ScheduledExtractWorker>>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _options = new ScheduledExtractOptions
            {
                ExtractIntervalMinutes = 1,
                MaxRetries = 3,
                RetryDelaySeconds = 1
            };

            _worker = new ScheduledExtractWorker(
                _loggerMock.Object,
                _serviceProviderMock.Object,
                Options.Create(_options));
        }

        [Fact]
        public async Task ExecuteScheduledExtractWithRetryAsync_ValidRequest_SucceedsOnFirstAttempt()
        {
            // Arrange
            var serviceMock = new Mock<IPowerReportService>();
            serviceMock.Setup(s => s.ProcessTradesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await InvokeRetryAsync(serviceMock.Object);

            // Assert
            serviceMock.Verify(s => s.ProcessTradesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
            _loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(lvl => lvl == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteScheduledExtractWithRetryAsync_FailsThenSucceeds_RetriesOnce()
        {
            // Arrange
            var serviceMock = new Mock<IPowerReportService>();
            int callCount = 0;
            serviceMock.Setup(s => s.ProcessTradesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    callCount++;
                    if (callCount == 1) throw new Exception("First failure");
                    return Task.CompletedTask;
                });

            // Act
            await InvokeRetryAsync(serviceMock.Object);

            // Assert
            serviceMock.Verify(s => s.ProcessTradesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(lvl => lvl == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteScheduledExtractWithRetryAsync_AllRetriesFail_LogsCriticalWithoutThrowing()
        {
            // Arrange
            var serviceMock = new Mock<IPowerReportService>();
            serviceMock.Setup(s => s.ProcessTradesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Always fails"));

            // Act
            await InvokeRetryAsync(serviceMock.Object);

            // Assert
            serviceMock.Verify(s => s.ProcessTradesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Exactly(_options.MaxRetries));

            _loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(lvl => lvl == LogLevel.Critical),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        private Task InvokeRetryAsync(IPowerReportService service)
        {
            var method = typeof(ScheduledExtractWorker).GetMethod("ExecuteScheduledExtractWithRetryAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method == null)
                throw new InvalidOperationException("Could not find ExecuteScheduledExtractWithRetryAsync method.");

            return (Task)method.Invoke(_worker, new object[] { service, CancellationToken.None })!;
        }
    }
}
