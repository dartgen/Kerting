namespace Kerting_Api.DTO
{
    public class PaginationParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 6;

        public void Validate()
        {
            if (Page < 1) Page = 1;
            if (PageSize < 1) PageSize = 6;
            if (PageSize > 100) PageSize = 100; // Max 100 items per page
        }
    }
}
