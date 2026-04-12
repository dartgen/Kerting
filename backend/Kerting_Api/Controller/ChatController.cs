using Libary.Model.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Kerting_Api.Interface;

namespace Kerting_Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    /// <summary>
    /// Chat vezérlő: beszélgetéslista, üzenetfolyam, szöveges és képes küldés végpontok.
    /// </summary>
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        // Kliens tokenjéből kinyert aktuális felhasználó azonosító.
        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirst("Id")?.Value;
            return int.Parse(userIdString);
        }

        /// <summary>
        /// Beszélgetéslista lekérése a bejelentkezett userhez.
        /// </summary>
        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<ChatListItemDto>>> GetConversations()
        {
            var userId = GetCurrentUserId();
            var dtos = await _chatService.GetConversationsAsync(userId);
            return Ok(dtos);
        }

        /// <summary>
        /// Üzenetek lekérése egy konkrét beszélgetéshez.
        /// </summary>
        [HttpGet("{conversationId}/messages")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(int conversationId)
        {
            var userId = GetCurrentUserId();
            try
            {
                var messages = await _chatService.GetMessagesAsync(userId, conversationId);
                return Ok(messages);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Új szöveges üzenet küldése.
        /// </summary>
        [HttpPost("send")]
        public async Task<ActionResult<MessageDto>> SendMessage([FromBody] SendMessageDto dto)
        {
            var userId = GetCurrentUserId();
            try
            {
                var message = await _chatService.SendMessageAsync(userId, dto);
                return Ok(message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Privát beszélgetés visszaadása vagy létrehozása két user között.
        /// </summary>
        [HttpPost("get-or-create/{targetUserId}")]
        public async Task<ActionResult<int>> GetOrCreateConversation(int targetUserId)
        {
            var currentUserId = GetCurrentUserId();
            try
            {
                var conversationId = await _chatService.GetOrCreateConversationAsync(currentUserId, targetUserId);
                return Ok(conversationId);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Kép küldése form-data kérésadatból.
        /// </summary>
        [HttpPost("send-image")]
        public async Task<ActionResult<MessageDto>> SendImage([FromForm] ImageUploadDto dto)
        {
            var userId = GetCurrentUserId();
            try
            {
                var message = await _chatService.SendImageAsync(userId, dto);
                return Ok(message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}