namespace Rpa.Worker.Configuration;

public sealed class WorkerOptions
{
    public const string SectionName = "Worker";

    
    public int IntervalMinutes { get; set; } = 15;
}
