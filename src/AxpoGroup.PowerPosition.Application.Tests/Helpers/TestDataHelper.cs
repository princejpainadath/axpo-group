using AxpoGroup.PowerPosition.Application.DTOs;

namespace AxpoGroup.PowerPosition.Application.Tests.Helpers
{
    public static class TestDataHelper
    {
        public static List<PowerTradeDto> GetValidTrades()
        {
            return new List<PowerTradeDto>
            {
                new PowerTradeDto
                {
                    Date = DateTime.Today,
                    Periods = new List<PowerPeriodDto>
                    {
                        new PowerPeriodDto { Period = 1, Volume = 100 },
                        new PowerPeriodDto { Period = 1, Volume = 50 },
                        new PowerPeriodDto { Period = 2, Volume = 200 },
                        new PowerPeriodDto { Period = 2, Volume = -20 }
                    }
                }
            };
        }
    }
}
