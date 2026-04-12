using Kerting_Api.Interface;
using Libary;
using Libary.Model.Auth;
using Libary.Model.Chat;
using Libary.Model.Project;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Kerting_Api.Service
{
    /// <summary>
    /// Chat üzleti logika.
    /// Kezeli a beszélgetéslistát, üzenetfolyamot, írási jogosultságokat és képküldési folyamatot.
    /// </summary>
    public sealed class ChatService : IChatService
    {
        private readonly KertingDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ChatService(KertingDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        /// <summary>
        /// User beszélgetéslistája.
        /// Az archivált projektekhez kötött chat-eket kiszűri, hogy lezárt projekthez ne jelenjen aktív csatorna.
        /// </summary>
        public async Task<IEnumerable<ChatListItemDto>> GetConversationsAsync(int userId)
        {
            var rawConversations = await _context.Conversations
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .Where(c => !_context.Set<Project>().Any(p => p.ChatConversationId == c.Id && p.Status == "archived"))
                .OrderByDescending(c => c.LastMessageAt)
                .Select(c => new
                {
                    c.Id,
                    c.IsGroup,
                    c.Title,
                    c.GroupImage,
                    Participants = c.Participants.Select(p => new
                    {
                        p.UserId,
                        p.User,
                        Username = _context.Set<Login>()
                            .Where(l => l.Id == p.UserId)
                            .Select(l => l.Username)
                            .FirstOrDefault()
                    }),
                    LastMessage = _context.Messages
                        .Where(m => m.ConversationId == c.Id)
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefault(),
                    UnreadCount = _context.Messages
                        .Count(m => m.ConversationId == c.Id && m.SenderId != userId && !m.IsRead)
                })
                .ToListAsync();

            // A nyers lekérdezés eredményét UI-barát DTO-vá formáljuk.
            return rawConversations.Select(c =>
            {
                var partner = c.Participants.FirstOrDefault(p => p.UserId != userId);
                var megjelenitendoNev = string.Empty;

                if (partner != null && partner.User != null)
                {
                    var teljesNev = (partner.User.VezetekNev + " " + partner.User.KeresztNev).Trim();
                    megjelenitendoNev = string.IsNullOrWhiteSpace(teljesNev)
                        ? (partner.Username ?? "Ismeretlen felhasználó")
                        : teljesNev;
                }

                return new ChatListItemDto
                {
                    Id = c.Id,
                    IsGroup = c.IsGroup,
                    Nev = c.IsGroup ? (c.Title ?? "Ismeretlen csoport") : megjelenitendoNev,
                    Avatar = c.IsGroup ? (c.GroupImage ?? string.Empty) : (partner?.User?.IMGString ?? string.Empty),
                    UtolsoUzenet = c.LastMessage != null ? c.LastMessage.Content : "Nincs üzenet",
                    UtolsoIdo = c.LastMessage != null ? c.LastMessage.CreatedAt.ToString("s") : string.Empty,
                    Olvasatlan = c.UnreadCount > 0
                };
            }).ToList();
        }

        /// <summary>
        /// Üzenetek lekérése egy beszélgetéshez.
        /// Jogosultságot ellenőriz, archivált projektchat-et tilt, majd az olvasatlanokat olvasottra jelöli.
        /// </summary>
        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(int userId, int conversationId)
        {
            var isParticipant = await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);
            if (!isParticipant)
            {
                throw new UnauthorizedAccessException("Nincs jogosultságod ehhez a beszélgetéshez.");
            }

            var isArchived = await _context.Set<Project>().AnyAsync(p => p.ChatConversationId == conversationId && p.Status == "archived");
            if (isArchived)
            {
                throw new InvalidOperationException("A projekt archiválva lett, a csevegés jelenleg szünetel.");
            }

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Szoveg = m.Content,
                    ImageUrl = m.ImageUrl,
                    Ido = m.CreatedAt.ToString("s"),
                    Sajat = m.SenderId == userId,
                    SenderName = string.IsNullOrWhiteSpace(m.Sender.KeresztNev)
                        ? _context.Set<Login>().Where(l => l.Id == m.SenderId).Select(l => l.Username).FirstOrDefault()
                        : m.Sender.KeresztNev
                })
                .ToListAsync();

            var unreadMessages = await _context.Messages
                .Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                // A beolvasott idegen üzeneteket olvasottra állítjuk.
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }

            return messages;
        }

        /// <summary>
        /// Új szöveges üzenet küldése jogosultság- és archivált-projekt ellenőrzéssel.
        /// </summary>
        public async Task<MessageDto> SendMessageAsync(int userId, SendMessageDto dto)
        {
            var isParticipant = await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == dto.ConversationId && cp.UserId == userId);
            if (!isParticipant)
            {
                throw new UnauthorizedAccessException("Nincs jogosultságod ide írni.");
            }

            var isArchived = await _context.Set<Project>().AnyAsync(p => p.ChatConversationId == dto.ConversationId && p.Status == "archived");
            if (isArchived)
            {
                throw new InvalidOperationException("A projekt archiválva lett, ide nem küldhetsz üzenetet.");
            }

            var newMessage = new Message
            {
                ConversationId = dto.ConversationId,
                SenderId = userId,
                Content = dto.Content,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(newMessage);

            var conversation = await _context.Conversations.FindAsync(dto.ConversationId);
            if (conversation != null)
            {
                // A legutolsó aktivitás idejét frissítjük a lista rendezéséhez.
                conversation.LastMessageAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return new MessageDto
            {
                Id = newMessage.Id,
                Szoveg = newMessage.Content,
                Ido = newMessage.CreatedAt.ToString("s"),
                Sajat = true,
                SenderName = string.Empty
            };
        }

        /// <summary>
        /// Két felhasználó közötti privát beszélgetés visszaadása,
        /// vagy új létrehozása, ha még nem létezik.
        /// </summary>
        public async Task<int> GetOrCreateConversationAsync(int currentUserId, int targetUserId)
        {
            if (currentUserId == targetUserId)
            {
                throw new InvalidOperationException("Nem indíthatsz beszélgetést saját magaddal.");
            }

            var existingConversationId = await _context.Conversations
                .Where(c => !c.IsGroup)
                .Where(c => c.Participants.Any(p => p.UserId == currentUserId) &&
                            c.Participants.Any(p => p.UserId == targetUserId))
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (existingConversationId != 0)
            {
                return existingConversationId;
            }

            var newConversation = new Conversation
            {
                IsGroup = false,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            _context.Conversations.Add(newConversation);
            await _context.SaveChangesAsync();

            _context.ConversationParticipants.AddRange(
                new ConversationParticipant { ConversationId = newConversation.Id, UserId = currentUserId },
                new ConversationParticipant { ConversationId = newConversation.Id, UserId = targetUserId }
            );

            await _context.SaveChangesAsync();
            return newConversation.Id;
        }

        /// <summary>
        /// Képes üzenet küldése:
        /// 1) jogosultság ellenőrzés,
        /// 2) fájl mentés Resources/ChatImages mappába,
        /// 3) Message rekord létrehozása.
        /// </summary>
        public async Task<MessageDto> SendImageAsync(int userId, ImageUploadDto dto)
        {
            var isParticipant = await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == dto.ConversationId && cp.UserId == userId);
            if (!isParticipant)
            {
                throw new UnauthorizedAccessException("Nincs jogosultságod ide írni.");
            }

            var isArchived = await _context.Set<Project>().AnyAsync(p => p.ChatConversationId == dto.ConversationId && p.Status == "archived");
            if (isArchived)
            {
                throw new InvalidOperationException("A projekt archiválva lett, ide nem küldhetsz képet.");
            }

            if (dto.Image == null || dto.Image.Length == 0)
            {
                throw new InvalidOperationException("Nincs kiválasztva kép.");
            }

            var uploadsFolder = Path.Combine(_env.ContentRootPath, "Resources", "ChatImages");
            if (!Directory.Exists(uploadsFolder))
            {
                // Első feltöltéskor automatikus mappalétrehozás.
                Directory.CreateDirectory(uploadsFolder);
            }

            var extension = Path.GetExtension(dto.Image.FileName);
            var uniqueFileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(fileStream);
            }

            var newMessage = new Message
            {
                ConversationId = dto.ConversationId,
                SenderId = userId,
                Content = "Fénykép",
                ImageUrl = uniqueFileName,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(newMessage);

            var conversation = await _context.Conversations.FindAsync(dto.ConversationId);
            if (conversation != null)
            {
                // A beszélgetés listában azonnal legelőre kerüljön activity alapján.
                conversation.LastMessageAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return new MessageDto
            {
                Id = newMessage.Id,
                Szoveg = newMessage.Content,
                ImageUrl = newMessage.ImageUrl,
                Ido = newMessage.CreatedAt.ToString("s"),
                Sajat = true,
                SenderName = string.Empty
            };
        }
    }
}