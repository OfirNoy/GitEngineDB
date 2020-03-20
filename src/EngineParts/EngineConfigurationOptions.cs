using Serilog;

namespace GitEngineDB.EngineParts
{
    public class EngineConfigurationOptions
    {
        public ILogger Logger { get; set; }
        public int WriteIntervals { get; set; } = 300;
    }
}
