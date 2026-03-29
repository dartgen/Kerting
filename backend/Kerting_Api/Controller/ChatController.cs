using Libary;
using Libary.Model.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kerting_Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly KertingDbContext _context; // Cseréld le a te DbContext osztályod nevére, ha más

        public ChatController(KertingDbContext context)
        {
            _context = context;
        }

        // Segédfüggvény: Kinyeri az aktuálisan bejelentkezett felhasználó ID-ját a Tokenből
        private int GetCurrentUserId() {

            var userIdString = User.FindFirst("Id")?.Value;
            return int.Parse(userIdString);

            throw new UnauthorizedAccessException("Érvénytelen felhasználó azonosító. Claim nem található.");
        }

        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<ChatListItemDto>>> GetConversations()
        {
            var userId = GetCurrentUserId();

            // 1. LÉPÉS: Lekérdezés az adatbázisból (Csak SQL-barát parancsok!)
            var rawConversations = await _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .OrderByDescending(c => c.LastMessageAt) // <--- Itt használjuk a gyors indexelt mezőt!
                .Select(c => new
                {
                    c.Id,
                    c.IsGroup,
                    c.Title,
                    c.GroupImage,
                    Participants = c.Participants,
                    // Csak magát az utolsó üzenet objektumot kérjük le
                    LastMessage = _context.Messages
                        .Where(m => m.ConversationId == c.Id)
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefault(),
                    // Kiszámoljuk az olvasatlanok számát
                    UnreadCount = _context.Messages
                        .Count(m => m.ConversationId == c.Id && m.SenderId != userId && !m.IsRead)
                })
                .ToListAsync(); // <-- Itt hajtódik végre az SQL kérés

            // 2. LÉPÉS: Formázás a memóriában (Itt már működik a .ToString és a C# logika)
            // 2. LÉPÉS: Formázás a memóriában
            var dtos = rawConversations.Select(c => new ChatListItemDto
            {
                Id = c.Id,
                IsGroup = c.IsGroup,

                // Név összerakása (Ha nincs Title, adunk egy alapértelmezettet)
                Nev = c.IsGroup
                    ? (c.Title ?? "Ismeretlen csoport")
                    : (c.Participants.FirstOrDefault(p => p.UserId != userId)?.User.VezetekNev + " " +
                       c.Participants.FirstOrDefault(p => p.UserId != userId)?.User.KeresztNev).Trim(),

                // Avatar (Ha NULL az adatbázisban, üres stringet adunk vissza a Vue-nak)
                Avatar = c.IsGroup
                    ? (c.GroupImage ?? "")
                    : (c.Participants.FirstOrDefault(p => p.UserId != userId)?.User.IMGString ?? ""),

                UtolsoUzenet = c.LastMessage != null ? c.LastMessage.Content : "Nincs üzenet",
                UtolsoIdo = c.LastMessage != null ? c.LastMessage.CreatedAt.ToString("HH:mm") : "",
                Olvasatlan = c.UnreadCount > 0
            }).ToList();

            return Ok(dtos);
        }

        // 2. Végpont: Egy kiválasztott beszélgetés üzeneteinek betöltése
        [HttpGet("{conversationId}/messages")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(int conversationId)
        {
            var userId = GetCurrentUserId();

            // BIZTONSÁG: Ellenőrizzük, hogy a kérdező tagja-e ennek a beszélgetésnek
            var isParticipant = await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            if (!isParticipant) return Forbid("Nincs jogosultságod ehhez a beszélgetéshez.");

            // Üzenetek betöltése
            var messages = await _context.Messages
                .Include(m => m.Sender) // Betöltjük a küldő adatait
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt) // Időrendbe (régi felül, új alul)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Szoveg = m.Content,
                    Ido = m.CreatedAt.ToString("HH:mm"),
                    Sajat = m.SenderId == userId,
                    SenderName = m.Sender.KeresztNev // Csoportosnál kelleni fog
                })
                .ToListAsync();

            // OLVASOTTÁ TÉTEL: Ami nekünk jött, azt most láttuk
            var unreadMessages = await _context.Messages
                .Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }

            return Ok(messages);
        }

        // 3. Végpont: Új üzenet küldése
        [HttpPost("send")]
        public async Task<ActionResult<MessageDto>> SendMessage([FromBody] SendMessageDto dto)
        {
            var userId = GetCurrentUserId();

            // BIZTONSÁG: Csak akkor írhat, ha tagja a szobának
            var isParticipant = await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == dto.ConversationId && cp.UserId == userId);

            if (!isParticipant) return Forbid("Nincs jogosultságod ide írni.");

            var newMessage = new Message
            {
                ConversationId = dto.ConversationId,
                SenderId = userId,
                Content = dto.Content,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(newMessage);

            // Frissítjük a beszélgetés idejét, hogy a listában legfelülre ugorjon
            var conversation = await _context.Conversations.FindAsync(dto.ConversationId);
            if (conversation != null)
            {
                conversation.LastMessageAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            // Válaszként visszaküldjük a friss üzenetet a Vue-nak (hogy egyből megjelenjen az ablakban)
            return Ok(new MessageDto
            {
                Id = newMessage.Id,
                Szoveg = newMessage.Content,
                Ido = newMessage.CreatedAt.ToString("HH:mm"),
                Sajat = true
            });
        }

        [HttpPost("get-or-create/{targetUserId}")]
        public async Task<ActionResult<int>> GetOrCreateConversation(int targetUserId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == targetUserId)
                return BadRequest("Nem indíthatsz beszélgetést saját magaddal.");

            // 1. Megnézzük, van-e már privát beszélgetés (IsGroup = false) ezen két ember között
            var existingConversationId = await _context.Conversations
                .Where(c => !c.IsGroup)
                .Where(c => c.Participants.Any(p => p.UserId == currentUserId) &&
                            c.Participants.Any(p => p.UserId == targetUserId))
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (existingConversationId != 0)
            {
                return Ok(existingConversationId); // Ha van, visszaadjuk az ID-ját
            }

            // 2. Ha nincs, létrehozunk egy újat
            var newConversation = new Conversation
            {
                IsGroup = false,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            _context.Conversations.Add(newConversation);
            await _context.SaveChangesAsync(); // Elmentjük, hogy kapjon egy ID-t

            // 3. Hozzáadjuk a két embert a résztvevők (Participants) táblához
            _context.ConversationParticipants.AddRange(
                new ConversationParticipant { ConversationId = newConversation.Id, UserId = currentUserId },
                new ConversationParticipant { ConversationId = newConversation.Id, UserId = targetUserId }
            );

            await _context.SaveChangesAsync();

            return Ok(newConversation.Id); // Visszaadjuk az új beszélgetés ID-ját
        }
    }
}
