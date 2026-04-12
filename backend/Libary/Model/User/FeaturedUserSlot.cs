namespace Libary.Model.User
{
    /// <summary>
    /// Kiemelt felhasználó slot rekord a kezdőoldali kiemeléshez.
    /// </summary>
    public class FeaturedUserSlot
    {
        public byte SlotNo { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        public User? User { get; set; }
    }
}
