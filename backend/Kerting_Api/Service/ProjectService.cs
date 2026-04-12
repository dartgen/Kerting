using Kerting_Api.Interface;
using Libary;
using Libary.Model.Project;
using Libary.Model.Chat;
using Microsoft.EntityFrameworkCore;
using Kerting_Api.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Kerting_Api.Service
{
    public class ProjectService : IProjectService
    {
        private readonly KertingDbContext _context;

        public ProjectService(KertingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(string userId)
        {
            var projects = await _context.Projects
                .Include(p => p.Members)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedTo)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Todos)
                .Where(p => p.OwnerId == userId || p.Members.Any(m => m.UserId == userId))
                .ToListAsync();

            var allUserIdsString = projects.SelectMany(p => p.Members.Select(m => m.UserId))
                .Concat(projects.Select(p => p.OwnerId))
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            var validUserIds = allUserIdsString
                .Where(id => int.TryParse(id, out _))
                .Select(int.Parse)
                .ToList();

            var userDetails = await _context.User
                .Where(u => validUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id.ToString(), u => new {
                    Name = (u.VezetekNev + " " + u.KeresztNev).Trim(),
                    Avatar = u.IMGString
                });

            var loginNames = await _context.Login
                .Where(l => validUserIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id.ToString(), l => l.Username);

            string GetDisplayName(string uid)
            {
                if (userDetails.TryGetValue(uid, out var user) && !string.IsNullOrWhiteSpace(user.Name))
                    return user.Name;
                if (loginNames.TryGetValue(uid, out var uname))
                    return uname;
                return "Ismeretlen";
            }

            string GetAvatar(string uid)
            {
                if (userDetails.TryGetValue(uid, out var user))
                    return user.Avatar;
                return null;
            }

            var result = projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                OwnerId = p.OwnerId,
                Title = p.Title ?? "Névtelen projekt",
                Description = p.Description ?? "",
                Deadline = p.Deadline?.ToString("yyyy-MM-dd"),

                // Itt adjuk át a chat szoba ID-ját a Vue-nak (a gombhoz)
                ChatConversationId = p.ChatConversationId,

                Status = p.OwnerId == userId ? (p.Status ?? "ongoing") :
                         (p.Members.FirstOrDefault(m => m.UserId == userId)?.Role == "Meghívott" ? "invited" : (p.Status ?? "ongoing")),

                Members = p.Members.Select(m => new ProjectMemberDto
                {
                    UserId = m.UserId,
                    Role = m.Role ?? "Tag",
                    Name = GetDisplayName(m.UserId),
                    Avatar = GetAvatar(m.UserId)
                }).ToList(),

                Tasks = p.Tasks.Select(t => new TaskDto
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    Title = t.Title ?? "Névtelen feladat",
                    Description = t.Description ?? "",
                    Amount = t.Amount,
                    Deadline = t.Deadline?.ToString("yyyy-MM-dd"),
                    Status = t.Status ?? "todo",
                    AssignedTo = t.AssignedTo.Select(a => a.UserId).ToList(),
                    Todos = t.Todos.Select(todo => new TodoDto
                    {
                        Id = todo.Id,
                        Text = todo.Text ?? "",
                        Amount = todo.Amount,
                        Completed = todo.Completed,
                        WorkerId = todo.WorkerId
                    }).ToList()
                }).ToList()
            }).ToList();

            return result;
        }

        public async Task<ProjectDto> CreateProjectAsync(string userId, ProjectDto dto)
        {
            var project = new Project
            {
                OwnerId = userId,
                Title = dto.Title,
                Description = dto.Description,
                Deadline = string.IsNullOrEmpty(dto.Deadline) ? null : DateTime.Parse(dto.Deadline),
                Status = "ongoing"
            };

            project.Members.Add(new ProjectMember
            {
                UserId = userId,
                Role = "Tulajdonos"
            });

            _context.Projects.Add(project);
            await _context.SaveChangesAsync(); // Elmentjük, hogy legyen ID-ja

            // ========================================================
            // AUTOMATIKUS CSOPORTOS CHAT LÉTREHOZÁSA A PROJEKTHEZ
            // ========================================================
            var newChat = new Conversation
            {
                IsGroup = true,
                Title = project.Title,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };
            _context.Conversations.Add(newChat);
            await _context.SaveChangesAsync();

            if (int.TryParse(userId, out int uId))
            {
                _context.ConversationParticipants.Add(new ConversationParticipant
                {
                    ConversationId = newChat.Id,
                    UserId = uId
                });

                _context.Messages.Add(new Message
                {
                    ConversationId = newChat.Id,
                    SenderId = uId,
                    Content = "A projekt létrejött, a csoportos csevegés elindult!",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });
            }

            // Összekötjük a projektet az új chattel
            project.ChatConversationId = newChat.Id;
            await _context.SaveChangesAsync();
            // ========================================================

            dto.Id = project.Id;
            dto.OwnerId = userId;
            if (dto.Members == null) dto.Members = new List<ProjectMemberDto>();
            dto.Members.Add(new ProjectMemberDto { UserId = userId, Role = "Tulajdonos", Name = "Én" });
            dto.ChatConversationId = newChat.Id;

            return dto;
        }

        public async Task<ProjectDto> UpdateProjectAsync(int projectId, ProjectDto dto)
        {
            var project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return null;

            // Ha változott a projekt neve, átnevezzük a hozzá tartozó csevegést is
            if (project.Title != dto.Title && project.ChatConversationId.HasValue)
            {
                var chat = await _context.Conversations.FindAsync(project.ChatConversationId.Value);
                if (chat != null) chat.Title = dto.Title;
            }

            project.Title = dto.Title;
            project.Description = dto.Description;
            project.Deadline = string.IsNullOrEmpty(dto.Deadline) ? null : DateTime.Parse(dto.Deadline);
            project.Status = dto.Status;

            // =========================================================================
            // TAGOK SZINKRONIZÁLÁSA: Ha kirúgtak valakit, a Chatből is kivesszük
            // =========================================================================
            if (dto.Members != null)
            {
                var incomingMemberIds = dto.Members.Select(m => m.UserId).ToList();

                var membersToRemove = project.Members
                    .Where(m => !incomingMemberIds.Contains(m.UserId) && m.Role != "Tulajdonos")
                    .ToList();

                foreach (var member in membersToRemove)
                {
                    project.Members.Remove(member);

                    if (project.ChatConversationId.HasValue && int.TryParse(member.UserId, out int removedUid))
                    {
                        var chatParticipant = await _context.ConversationParticipants
                            .FirstOrDefaultAsync(cp => cp.ConversationId == project.ChatConversationId.Value && cp.UserId == removedUid);

                        if (chatParticipant != null)
                        {
                            _context.ConversationParticipants.Remove(chatParticipant);
                        }
                    }
                }
            }
            // =========================================================================

            await _context.SaveChangesAsync();
            return dto;
        }

        public async Task DeleteProjectAsync(int projectId, string userId)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == userId);
            if (project != null)
            {
                // Ha törlik a projektet, eltüntetjük a hozzá tartozó chat szobát is
                if (project.ChatConversationId.HasValue)
                {
                    var chat = await _context.Conversations.FindAsync(project.ChatConversationId.Value);
                    if (chat != null) _context.Conversations.Remove(chat);
                }

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
        }

        // ==========================================
        // --- FELADATOK (TASKS) KEZELÉSE ---
        // ==========================================
        public async Task<TaskDto> SaveTaskAsync(int projectId, TaskDto taskDto)
        {
            ProjectTask task;

            if (taskDto.Id > 0)
            {
                task = await _context.ProjectTasks
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Todos)
                    .FirstOrDefaultAsync(t => t.Id == taskDto.Id && t.ProjectId == projectId);

                if (task == null) throw new Exception("Task not found");

                task.Title = taskDto.Title;
                task.Description = taskDto.Description;
                task.Amount = taskDto.Amount;
                task.Status = taskDto.Status;
                task.Deadline = string.IsNullOrEmpty(taskDto.Deadline) ? null : DateTime.Parse(taskDto.Deadline);

                if (taskDto.Todos != null)
                {
                    var incomingTodoIds = taskDto.Todos.Where(t => t.Id > 0).Select(t => t.Id).ToList();
                    var todosToRemove = task.Todos.Where(t => !incomingTodoIds.Contains(t.Id)).ToList();
                    foreach (var r in todosToRemove) task.Todos.Remove(r);

                    foreach (var todoDto in taskDto.Todos)
                    {
                        if (todoDto.Id > 0)
                        {
                            var existingTodo = task.Todos.FirstOrDefault(t => t.Id == todoDto.Id);
                            if (existingTodo != null)
                            {
                                existingTodo.Text = todoDto.Text;
                                existingTodo.Amount = todoDto.Amount;
                                existingTodo.Completed = todoDto.Completed;
                                existingTodo.WorkerId = todoDto.WorkerId;
                            }
                        }
                        else
                        {
                            task.Todos.Add(new TodoItem
                            {
                                Text = todoDto.Text,
                                Amount = todoDto.Amount,
                                Completed = todoDto.Completed,
                                WorkerId = todoDto.WorkerId
                            });
                        }
                    }
                }

                var toRemoveAssign = task.AssignedTo.Where(a => !taskDto.AssignedTo.Contains(a.UserId)).ToList();
                foreach (var r in toRemoveAssign) task.AssignedTo.Remove(r);

                var existingUsers = task.AssignedTo.Select(a => a.UserId).ToList();
                foreach (var uid in taskDto.AssignedTo.Where(id => !existingUsers.Contains(id)))
                {
                    task.AssignedTo.Add(new TaskAssignment { UserId = uid });
                }
            }
            else
            {
                task = new ProjectTask
                {
                    ProjectId = projectId,
                    Title = taskDto.Title,
                    Description = taskDto.Description,
                    Amount = taskDto.Amount,
                    Status = "todo",
                    Deadline = string.IsNullOrEmpty(taskDto.Deadline) ? null : DateTime.Parse(taskDto.Deadline)
                };

                if (taskDto.AssignedTo != null)
                {
                    foreach (var uid in taskDto.AssignedTo) task.AssignedTo.Add(new TaskAssignment { UserId = uid });
                }

                if (taskDto.Todos != null)
                {
                    foreach (var t in taskDto.Todos)
                    {
                        task.Todos.Add(new TodoItem
                        {
                            Text = t.Text,
                            Amount = t.Amount,
                            Completed = t.Completed,
                            WorkerId = t.WorkerId
                        });
                    }
                }

                _context.ProjectTasks.Add(task);
            }

            await _context.SaveChangesAsync();
            taskDto.Id = task.Id;

            if (task.Todos != null)
            {
                taskDto.Todos = task.Todos.Select(todo => new TodoDto
                {
                    Id = todo.Id,
                    Text = todo.Text,
                    Amount = todo.Amount,
                    Completed = todo.Completed,
                    WorkerId = todo.WorkerId
                }).ToList();
            }

            return taskDto;
        }

        public async Task DeleteTaskAsync(int projectId, int taskId)
        {
            var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);
            if (task != null)
            {
                _context.ProjectTasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        // ==========================================
        // --- MEGHÍVÓK KEZELÉSE ---
        // ==========================================
        public async Task InviteMemberAsync(int projectId, string userIdToInvite)
        {
            var project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project != null && !project.Members.Any(m => m.UserId == userIdToInvite))
            {
                project.Members.Add(new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = userIdToInvite,
                    Role = "Meghívott"
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task AcceptInviteAsync(int projectId, string userId)
        {
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);

            if (member != null)
            {
                member.Role = "Tag";

                // =========================================================================
                // CHATBE HELYEZÉS: Amint elfogadta a meghívót, bekerül a szobába is
                // =========================================================================
                var project = await _context.Projects.FindAsync(projectId);

                if (project != null && project.ChatConversationId.HasValue && int.TryParse(userId, out int uId))
                {
                    var alreadyInChat = await _context.ConversationParticipants
                        .AnyAsync(cp => cp.ConversationId == project.ChatConversationId.Value && cp.UserId == uId);

                    if (!alreadyInChat)
                    {
                        _context.ConversationParticipants.Add(new ConversationParticipant
                        {
                            ConversationId = project.ChatConversationId.Value,
                            UserId = uId
                        });

                        _context.Messages.Add(new Message
                        {
                            ConversationId = project.ChatConversationId.Value,
                            SenderId = uId,
                            Content = "Csatlakoztam a projekthez!",
                            CreatedAt = DateTime.Now,
                            IsRead = false
                        });
                    }
                }
                // =========================================================================

                await _context.SaveChangesAsync();
            }
        }

        public async Task RejectInviteAsync(int projectId, string userId)
        {
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);

            if (member != null)
            {
                _context.ProjectMembers.Remove(member);
                await _context.SaveChangesAsync();
            }
        }
    }
}