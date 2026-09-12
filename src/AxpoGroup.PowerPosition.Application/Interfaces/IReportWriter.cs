using AxpoGroup.PowerPosition.Application.DTOs;

namespace AxpoGroup.PowerPosition.Application.Interfaces
{
    public interface IReportWriter
    {
        void Export(IEnumerable<PowerPositionDto> positions, CancellationToken cancellationToken);
    }
}
