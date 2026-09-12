using Axpo;
using AxpoGroup.PowerPosition.Application.DTOs;

namespace AxpoGroup.PowerPosition.Infrastructure.Tests.Helpers
{
    public static class TestDataHelper
    {
        public static List<PowerTrade> GetValidTrades(DateTime date)
        {
            var trades = new List<PowerTrade>();
            for (int i = 0; i < 3; i++)
            {
                trades.Add(PowerTrade.Create(date, 24));
            }
            return trades;
        }

        public static List<PowerPositionDto> GetValidPositions()
        {
            return new List<PowerPositionDto>
            {
                new PowerPositionDto { LocalTime = "23:00", Volume = 150 },
                new PowerPositionDto { LocalTime = "00:00", Volume = 200 }
            };
        }
    }
}
