using AutoMapper;
using Axpo;
using AxpoGroup.PowerPosition.Application.DTOs;
using AxpoGroup.PowerPosition.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AxpoGroup.PowerPosition.Infrastructure.Repositories
{
    public class PowerTradeRepository : IPowerTradeRepository
    {
        private readonly ILogger<PowerTradeRepository> _logger;
        private readonly IPowerService _powerService;
        private readonly IMapper _mapper;

        public PowerTradeRepository(
            ILogger<PowerTradeRepository> logger,
            IPowerService powerService,
            IMapper mapper)
        {
            _powerService = powerService;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves trades for the specified date from the power service and maps them into a collection of trade DTOs.
        /// </summary>
        /// <param name="date">The date for which trades should be retrieved.</param>
        /// <returns>A task representing the asynchronous operation, containing a collection of trade DTOs for the given date.</returns>
        public async Task<IEnumerable<PowerTradeDto>> GetTradesAsync(DateTime date)
        {
            try
            {
                var trades = await _powerService.GetTradesAsync(date);
                return _mapper.Map<IEnumerable<PowerTradeDto>>(trades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{function} failed to get trades for {date}", nameof(GetTradesAsync), date);
                throw;
            }
        }
    }
}
