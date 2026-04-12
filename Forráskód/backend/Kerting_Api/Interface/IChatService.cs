using Libary.Model.Chat;

namespace Kerting_Api.Interface
{
    /// <summary>
    /// Chat modul szerződése beszélgetésekhez és üzenetekhez.
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// A user beszélgetéslistája a bal oldali chat panelhez.
        /// </summary>
        Task<IEnumerable<ChatListItemDto>> GetConversationsAsync(int userId);

        /// <summary>
        /// Egy beszélgetés üzenetei + olvasatlan jelzések frissítése.
        /// </summary>
        Task<IEnumerable<MessageDto>> GetMessagesAsync(int userId, int conversationId);

        /// <summary>
        /// Új szöveges üzenet küldése.
        /// </summary>
        Task<MessageDto> SendMessageAsync(int userId, SendMessageDto dto);

        /// <summary>
        /// Két felhasználó közti privát beszélgetés lekérése vagy létrehozása.
        /// </summary>
        Task<int> GetOrCreateConversationAsync(int currentUserId, int targetUserId);

        /// <summary>
        /// Képes üzenet küldése multipart kérésadatból.
        /// </summary>
        Task<MessageDto> SendImageAsync(int userId, ImageUploadDto dto);
    }
}