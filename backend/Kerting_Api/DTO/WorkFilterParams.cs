namespace Kerting_Api.DTO
{
    public class WorkFilterParams
    {
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string? TargetAudience { get; set; }
        public string? Status { get; set; } // Comma-separated: "Open,Public,InProgress"

        public List<string> GetStatusList()
        {
            if (string.IsNullOrWhiteSpace(Status))
                return new();
            return Status.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }
    }
}
