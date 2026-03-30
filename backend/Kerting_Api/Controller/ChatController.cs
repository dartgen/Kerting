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
        private readonly KertingDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ChatController(KertingDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirst("Id")?.Value;
            return int.Parse(userIdString);
        }

        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<ChatListItemDto>>> GetConversations()
        {
            var userId = GetCurrentUserId();

            var rawConversations = await _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .OrderByDescending(c => c.LastMessageAt)
                .Select(c => new
                {
                    c.Id,
                    c.IsGroup,
                    c.Title,
                    c.GroupImage,
                    Participants = c.Participants,
                    LastMessage = _context.Messages
                        .Where(m => m.ConversationId == c.Id)
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefault(),
                    UnreadCount = _context.Messages
                        .Count(m => m.ConversationId == c.Id && m.SenderId != userId && !m.IsRead)
                })
                .ToListAsync();

            var dtos = rawConversations.Select(c => new ChatListItemDto
            {
                Id = c.Id,
                IsGroup = c.IsGroup,
                Nev = c.IsGroup
                    ? (c.Title ?? "Ismeretlen csoport")
                    : (c.Participants.FirstOrDefault(p => p.UserId != userId)?.User.VezetekNev + " " +
                       c.Participants.FirstOrDefault(p => p.UserId != userId)?.User.KeresztNev).Trim(),
                Avatar = c.IsGroup
                    ? (c.GroupImage ?? "")
                    : (c.Participants.FirstOrDefault(p => p.UserId != userId)?.User.IMGString ?? ""),
                UtolsoUzenet = c.LastMessage != null ? c.LastMessage.Content : "Nincs üzenet",
                UtolsoIdo = c.LastMessage != null ? c.LastMessage.CreatedAt.ToString("HH:mm") : "",
                Olvasatlan = c.UnreadCount > 0
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{conversationId}/messages")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(int conversationId)
        {
            var userId = GetCurrentUserId();

            var isParticipant = await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            if (!isParticipant) return Forbid("Nincs jogosultságod ehhez a beszélgetéshez.");

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Szoveg = m.Content,
                    ImageUrl = m.ImageUrl,
                    Ido = m.CreatedAt.ToString("HH:mm"),
                    Sajat = m.SenderId == userId,
                    SenderName = m.Sender.KeresztNev
                })
                .ToListAsync();

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

        [HttpPost("send")]
        public async Task<ActionResult<MessageDto>> SendMessage([FromBody] SendMessageDto dto)
        {
            var userId = GetCurrentUserId();

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

            var conversation = await _context.Conversations.FindAsync(dto.ConversationId);
            if (conversation != null)
            {
                conversation.LastMessageAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

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

            var existingConversationId = await _context.Conversations
                .Where(c => !c.IsGroup)
                .Where(c => c.Participants.Any(p => p.UserId == currentUserId) &&
                            c.Participants.Any(p => p.UserId == targetUserId))
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (existingConversationId != 0)
            {
                return Ok(existingConversationId);
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

            return Ok(newConversation.Id);
        }

        [HttpPost("send-image")]
        public async Task<ActionResult<MessageDto>> SendImage([FromForm] ImageUploadDto dto)
        {
            var userId = GetCurrentUserId();

            int conversationId = dto.ConversationId;
            IFormFile image = dto.Image;

            var isParticipant = await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            if (!isParticipant) return Forbid("Nincs jogosultságod ide írni.");
            if (image == null || image.Length == 0) return BadRequest("Nincs kiválasztva kép.");

            // A képmentés pontosan a 'Resources/ChatImages' mappába kerül!
            string uploadsFolder = Path.Combine(_env.ContentRootPath, "Resources", "ChatImages");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            var newMessage = new Message
            {
                ConversationId = conversationId,
                SenderId = userId,
                Content = "Fénykép",
                ImageUrl = uniqueFileName,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(newMessage);

            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation != null) conversation.LastMessageAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new MessageDto
            {
                Id = newMessage.Id,
                Szoveg = newMessage.Content,
                ImageUrl = newMessage.ImageUrl,
                Ido = newMessage.CreatedAt.ToString("HH:mm"),
                Sajat = true
            });
        }
    }
}