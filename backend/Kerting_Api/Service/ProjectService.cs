using Kerting_Api.Interface;
using Libary;
using Libary.Model.Project;
using Microsoft.EntityFrameworkCore;
using Kerting_Api.DTO;

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
            // 1. Lekérdezzük az adatbázisból a projekteket
            var projects = await _context.Projects
                .Include(p => p.Members)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedTo)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Todos)
                .Where(p => p.OwnerId == userId || p.Members.Any(m => m.UserId == userId))
                .ToListAsync();

            // =========================================================================
            // --- ÚJ RÉSZ: Kigyűjtjük a felhasználókat, hogy a valós nevüket adjuk át ---
            // =========================================================================
            var allUserIdsString = projects.SelectMany(p => p.Members.Select(m => m.UserId))
                .Concat(projects.Select(p => p.OwnerId))
                .Distinct()
                .ToList();

            var validUserIds = allUserIdsString
                .Where(id => int.TryParse(id, out _))
                .Select(int.Parse)
                .ToList();

            // Nevek és profilképek lekérése a User táblából
            var userDetails = await _context.User
                .Where(u => validUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id.ToString(), u => new {
                    Name = (u.VezetekNev + " " + u.KeresztNev).Trim(),
                    Avatar = u.IMGString
                });

            // Felhasználónevek lekérése a Login táblából (ha nincs kitöltve a rendes név)
            var loginNames = await _context.Login
                .Where(l => validUserIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id.ToString(), l => l.Username);

            // Belső segédfüggvény a név eldöntésére
            string GetDisplayName(string uid)
            {
                if (userDetails.TryGetValue(uid, out var user) && !string.IsNullOrWhiteSpace(user.Name))
                    return user.Name;
                if (loginNames.TryGetValue(uid, out var uname))
                    return uname;
                return "Ismeretlen";
            }

            // Belső segédfüggvény a profilképhez
            string GetAvatar(string uid)
            {
                if (userDetails.TryGetValue(uid, out var user))
                    return user.Avatar;
                return null;
            }
            // =========================================================================

            // 2. Átmappeljük a C# entitásokat
            var result = projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                OwnerId = p.OwnerId,
                Title = p.Title,
                Description = p.Description,
                Deadline = p.Deadline?.ToString("yyyy-MM-dd"),

                Status = p.OwnerId == userId ? p.Status :
                         (p.Members.FirstOrDefault(m => m.UserId == userId)?.Role == "Meghívott" ? "invited" : p.Status),

                Members = p.Members.Select(m => new ProjectMemberDto
                {
                    UserId = m.UserId,
                    Role = m.Role,
                    Name = GetDisplayName(m.UserId), // <--- ITT A JAVÍTÁS! "Tag" helyett a valós név!
                    Avatar = GetAvatar(m.UserId)     // <--- Profilkép átadása a Vue-nak!
                }).ToList(),

                Tasks = p.Tasks.Select(t => new TaskDto
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    Title = t.Title,
                    Description = t.Description,
                    Amount = t.Amount,
                    Deadline = t.Deadline?.ToString("yyyy-MM-dd"),
                    Status = t.Status,
                    AssignedTo = t.AssignedTo.Select(a => a.UserId).ToList(),
                    Todos = t.Todos.Select(todo => new TodoDto
                    {
                        Id = todo.Id,
                        Text = todo.Text,
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

            // A létrehozót automatikusan felvesszük a projekt tagjai közé "Tulajdonos" ranggal!
            project.Members.Add(new ProjectMember
            {
                UserId = userId,
                Role = "Tulajdonos"
            });

            // Ha jöttek további tagok a Vue felületről
            if (dto.Members != null)
            {
                foreach (var member in dto.Members)
                {
                    // Biztosítjuk, hogy önmagunkat ne adjuk hozzá duplán
                    if (member.UserId != userId)
                    {
                        project.Members.Add(new ProjectMember
                        {
                            UserId = member.UserId,
                            Role = member.Role
                        });
                    }
                }
            }

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // A Vue-nak visszaküldött DTO-ba is beleégetjük a helyes adatokat
            dto.Id = project.Id;
            dto.OwnerId = userId;
            if (dto.Members == null) dto.Members = new List<ProjectMemberDto>();
            dto.Members.Add(new ProjectMemberDto { UserId = userId, Role = "Tulajdonos", Name = "Én" });

            return dto;
        }

        public async Task<ProjectDto> UpdateProjectAsync(int projectId, ProjectDto dto)
        {
            var project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return null;

            project.Title = dto.Title;
            project.Description = dto.Description;
            project.Deadline = string.IsNullOrEmpty(dto.Deadline) ? null : DateTime.Parse(dto.Deadline);
            project.Status = dto.Status;

            await _context.SaveChangesAsync();
            return dto;
        }

        public async Task DeleteProjectAsync(int projectId, string userId)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == userId);
            if (project != null)
            {
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
                // Meglévő feladat betöltése (a Todos-t is be kell tölteni a módosításhoz!)
                task = await _context.ProjectTasks
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Todos)
                    .FirstOrDefaultAsync(t => t.Id == taskDto.Id && t.ProjectId == projectId);

                if (task == null) throw new Exception("Task not found");

                // 1. Alapadatok frissítése
                task.Title = taskDto.Title;
                task.Description = taskDto.Description;
                task.Amount = taskDto.Amount;
                task.Status = taskDto.Status;
                task.Deadline = string.IsNullOrEmpty(taskDto.Deadline) ? null : DateTime.Parse(taskDto.Deadline);

                // 2. ÚJ RÉSZ: Részfeladatok (TODOS) mentése!
                if (taskDto.Todos != null)
                {
                    // A) Mik azok a meglévő ID-k, amik jöttek a Vue-tól? (Ami 0, az teljesen új!)
                    var incomingTodoIds = taskDto.Todos.Where(t => t.Id > 0).Select(t => t.Id).ToList();

                    // B) Töröljük azokat az adatbázisból, amik már nincsenek a Vue listájában (törölted a felületen)
                    var todosToRemove = task.Todos.Where(t => !incomingTodoIds.Contains(t.Id)).ToList();
                    foreach (var r in todosToRemove)
                    {
                        task.Todos.Remove(r);
                    }

                    // C) Frissítjük a meglévőket, vagy hozzáadjuk az újakat (ahol Id == 0)
                    foreach (var todoDto in taskDto.Todos)
                    {
                        if (todoDto.Id > 0) // Meglévő módosítása (pl. kipipáltad)
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
                        else // Új hozzáadása (a Vue 0 ID-t küldött)
                        {
                            task.Todos.Add(new TodoItem // <--- ITT A JAVÍTÁS: ProjectTaskTodo helyett TodoItem
                            {
                                Text = todoDto.Text,
                                Amount = todoDto.Amount,
                                Completed = todoDto.Completed,
                                WorkerId = todoDto.WorkerId
                            });
                        }
                    }
                }

                // 3. Felelősök szinkronizálása
                var toRemoveAssign = task.AssignedTo.Where(a => !taskDto.AssignedTo.Contains(a.UserId)).ToList();
                foreach (var r in toRemoveAssign) task.AssignedTo.Remove(r);

                var existingUsers = task.AssignedTo.Select(a => a.UserId).ToList();
                foreach (var userId in taskDto.AssignedTo.Where(id => !existingUsers.Contains(id)))
                {
                    task.AssignedTo.Add(new TaskAssignment { UserId = userId });
                }
            }
            else
            {
                // TELJESEN ÚJ FELADAT LÉTREHOZÁSA
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
                    foreach (var userId in taskDto.AssignedTo) task.AssignedTo.Add(new TaskAssignment { UserId = userId });
                }

                // Ha egyből részfeladatokkal hozná létre
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

            // DTO visszaküldése a Vue-nak a friss adatbázis ID-kkal!
            taskDto.Id = task.Id;

            if (task.Todos != null)
            {
                taskDto.Todos = task.Todos.Select(todo => new TodoDto
                {
                    Id = todo.Id, // Itt már a valódi, adatbázis által generált ID-t küldjük vissza!
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

            // Ha létezik a projekt, és az illető még nincs benne
            if (project != null && !project.Members.Any(m => m.UserId == userIdToInvite))
            {
                project.Members.Add(new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = userIdToInvite,
                    Role = "Meghívott" // Ő még csak meghívott!
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
                member.Role = "Tag"; // Átírjuk Tag-ra, hivatalosan is bekerült!
                await _context.SaveChangesAsync();
            }
        }

        public async Task RejectInviteAsync(int projectId, string userId)
        {
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);

            if (member != null)
            {
                _context.ProjectMembers.Remove(member); // Töröljük a kapcsolatot
                await _context.SaveChangesAsync();
            }
        }
    }
}