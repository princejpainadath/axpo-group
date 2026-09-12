namespace AxpoGroup.PowerPosition.Application.Options
{
    public class CsvExportOptions
    {
        public string OutputDirectory { get; set; } = string.Empty;
        public string FileNamePattern { get; set; } = string.Empty;
        public string TimestampFormat { get; set; } = string.Empty;
    }
}
