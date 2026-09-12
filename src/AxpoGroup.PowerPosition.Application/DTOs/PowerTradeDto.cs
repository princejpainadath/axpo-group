namespace AxpoGroup.PowerPosition.Application.DTOs
{
    public class PowerTradeDto
    {
        public DateTime Date { get; set; }
        public List<PowerPeriodDto> Periods { get; set; } = new();
    }
}
