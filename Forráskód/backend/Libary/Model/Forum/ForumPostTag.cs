namespace Libary.Model.Forum
{
    /// <summary>
    /// Fórum bejegyzés és aktivitási címke közötti kapcsolótábla entitás.
    /// </summary>
    public class ForumPostTag
    {
        public int ForumPostId { get; set; }
        public int TagId { get; set; }

        public ForumPost ForumPost { get; set; } = null!;
        public Tag.ActivityTag ActivityTag { get; set; } = null!;
    }
}
