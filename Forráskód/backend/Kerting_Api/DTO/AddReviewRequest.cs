using System.ComponentModel.DataAnnotations;

namespace Kerting_Api.DTO
{
    /// <summary>
    /// Új felhasználói értékelés vagy válasz beküldésének kérésmodellje.
    /// </summary>
    public sealed class AddReviewRequest
    {
        /// <summary>
        /// Opcionális szülő értékelés azonosító; ha meg van adva, ez egy reply.
        /// </summary>
        public int? ParentReviewId { get; set; }

        /// <summary>
        /// Opcionális csillag érték (1-5), válasz típusú bejegyzésnél lehet null.
        /// </summary>
        public byte? Rating { get; set; }

        /// <summary>
        /// Kötelező szöveges üzenet az értékeléshez.
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;
    }
}