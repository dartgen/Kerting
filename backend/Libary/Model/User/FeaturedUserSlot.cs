namespace Libary.Model.User
{
    public class FeaturedUserSlot
    {
        public byte SlotNo { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        public User? User { get; set; }
    }
}
