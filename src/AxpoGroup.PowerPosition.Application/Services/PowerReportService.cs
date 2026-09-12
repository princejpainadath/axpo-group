using AutoMapper;
using AxpoGroup.PowerPosition.Application.DTOs;
using AxpoGroup.PowerPosition.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AxpoGroup.PowerPosition.Application.Services
{
    public class PowerReportService : IPowerReportService
    {
        private readonly ILogger<PowerReportService> _logger;
        private readonly IPowerTradeRepository _repository;
        private readonly IReportWriter _reportWriter;
        private readonly IMapper _mapper;

        public PowerReportService(
            ILogger<PowerReportService> logger,
            IPowerTradeRepository repository,
            IReportWriter reportWriter,
            IMapper mapper)
        {
            _logger = logger;
            _repository = repository;
            _reportWriter = reportWriter;
            _mapper = mapper;
        }

        /// <summary>
        /// Executes the trade processing workflow for a given date.
        /// Retrieves trades, aggregates them into positions, and writes the results to CSV.
        /// </summary>
        /// <param name="date">The date for which trades should be retrieved and processed.</param>
        /// <param name="cancellationToken">A cancellation token that signals when the operation should stop.</param>
        /// <returns></returns>
        public async Task ProcessTradesAsync(
            DateTime date,
            CancellationToken cancellationToken)
        {
            try
            {
                var trades = await _repository.GetTradesAsync(date);

                if (trades == null || !trades.Any())
                {
                    _logger.LogWarning("{function} found no trades for date {date}.", nameof(ProcessTradesAsync), date);
                    return;
                }

                var powerPositions = AggregateTrades(trades);
                var powerPositionDtos = _mapper.Map<IEnumerable<PowerPositionDto>>(powerPositions);
                _reportWriter.Export(powerPositionDtos, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{function} failed for date {date}.", nameof(ProcessTradesAsync), date);
                throw;
            }
        }

        /// <summary>
        /// Aggregates a collection of power trades into positions by summing volumes for each local time period.
        /// </summary>
        /// <param name="trades">The collection of power trades to process and aggregate.</param>
        /// <returns>A collection of aggregated power positions.</returns>
        private IEnumerable<Domain.Entities.PowerPosition> AggregateTrades(IEnumerable<PowerTradeDto> trades)
        {
            try
            {
                var powerPositions = new List<Domain.Entities.PowerPosition>();

                foreach (var trade in trades)
                {
                    foreach (var period in trade.Periods)
                    {
                        var localTime = CalculateLocalTime(period.Period);
                        var existing = powerPositions.FirstOrDefault(p => p.LocalTime == localTime);

                        if (existing == null)
                        {
                            powerPositions.Add(new Domain.Entities.PowerPosition
                            {
                                LocalTime = localTime,
                                Volume = period.Volume
                            });
                        }
                        else
                            existing.Volume += period.Volume;
                    }
                }

                return powerPositions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{function} failed while aggregating trades.", nameof(AggregateTrades));
                throw;
            }
        }

        /// <summary>
        /// Calculates the local time string (HH:mm) for a given period number.
        /// Periods start at 23:00 of the previous day, with each period representing one hour.
        /// </summary>
        /// <param name="periodNumber">The 1-based period number to convert into a local time.</param>
        /// <returns>The local time string.</returns>
        private string CalculateLocalTime(int periodNumber)
        {
            try
            {
                var start = DateTime.Today.AddHours(-1);
                var time = start.AddHours(periodNumber - 1);
                return time.ToString("HH:mm");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{function} failed for period {periodNumber}.", nameof(CalculateLocalTime), periodNumber);
                throw;
            }
        }
    }
}
