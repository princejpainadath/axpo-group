namespace AxpoGroup.PowerPosition.Application.Interfaces
{
    public interface IPowerReportService
    {
        Task ProcessTradesAsync(DateTime date, CancellationToken cancellationToken);
    }
}
