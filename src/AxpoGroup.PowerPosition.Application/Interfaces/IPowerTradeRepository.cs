using AxpoGroup.PowerPosition.Application.DTOs;


namespace AxpoGroup.PowerPosition.Application.Interfaces
{
    public interface IPowerTradeRepository
    {
        Task<IEnumerable<PowerTradeDto>> GetTradesAsync(DateTime date);
    }
}
