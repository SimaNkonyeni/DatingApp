namespace API.Helpers
{
    public class VisitsParams: PaginationParams
    {
        public int UserId { get; set; }
        public string Predicate { get; set; }
    }
} 