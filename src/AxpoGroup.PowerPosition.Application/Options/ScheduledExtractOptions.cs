using System.ComponentModel.DataAnnotations;

namespace AxpoGroup.PowerPosition.Application.Options
{
    public class ScheduledExtractOptions
    {
        [Range(1, int.MaxValue)]
        public int ExtractIntervalMinutes { get; set; }
        [Range(1, 10)]
        public int MaxRetries { get; set; }
        [Range(1, 60)]
        public int RetryDelaySeconds { get; set; }
    }
}
