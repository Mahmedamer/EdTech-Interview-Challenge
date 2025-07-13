namespace StudentProgressTracker.Configuration
{
    public class CacheOptions
    {
        public const string SectionName = "Cache";
        
        public CacheProvider Provider { get; set; } = CacheProvider.Hybrid;
        public string? ConnectionString { get; set; }
        public string InstanceName { get; set; } = "StudentProgressTracker";
        public int DefaultExpirationMinutes { get; set; } = 60;
    }

    public enum CacheProvider
    {
        Memory,
        Redis,
        Hybrid
    }
}
