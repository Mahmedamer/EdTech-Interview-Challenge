namespace StudentProgressTracker.DTOs
{
    public static class ReportDto
    {
        public class ExportOptionsDto
        {
            public string Format { get; set; } = "csv";
            public int? Grade { get; set; }
            public string? Subject { get; set; }
            public bool IncludeProgress { get; set; } = true;
            public bool IncludeAssignments { get; set; } = false;
        }

        public class StudentReportOptionsDto
        {
            public int StudentId { get; set; }
            public string Format { get; set; } = "pdf";
            public bool IncludeHistory { get; set; } = true;
            public bool IncludeRecommendations { get; set; } = true;
        }

        public class ClassReportOptionsDto
        {
            public string Format { get; set; } = "pdf";
            public int? Grade { get; set; }
            public string? Subject { get; set; }
            public bool IncludeAnalytics { get; set; } = true;
        }

        public class ExportResultDto
        {
            public byte[] Data { get; set; } = Array.Empty<byte>();
            public string FileName { get; set; } = string.Empty;
            public string ContentType { get; set; } = string.Empty;
            public int RecordCount { get; set; }
            public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        }

        public class ReportResultDto
        {
            public byte[] Data { get; set; } = Array.Empty<byte>();
            public string FileName { get; set; } = string.Empty;
            public string ContentType { get; set; } = string.Empty;
            public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
            public Dictionary<string, object> Metadata { get; set; } = new();
        }
    }
}
