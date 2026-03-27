namespace Libary.Model.Forum
{
    public class ForumPostTag
    {
        public int ForumPostId { get; set; }
        public int TagId { get; set; }

        public ForumPost ForumPost { get; set; } = null!;
        public Tag.ActivityTag ActivityTag { get; set; } = null!;
    }
}
