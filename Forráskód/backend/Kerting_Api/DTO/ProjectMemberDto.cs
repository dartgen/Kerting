namespace Kerting_Api.DTO
{
    public class ProjectMemberDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Avatar { get; set; } // <--- Ezt a sort add hozzá!
    }
}
