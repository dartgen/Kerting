using Kerting_Api.Interface;
using Kerting_Api.DTO;
using Libary;
using Libary.Model.Work;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Kerting_Api.Service
{
    public class WorkService : IWorkService
    {
        private readonly GenericInterface<Work> _workRepo;
        private readonly GenericInterface<WorkApplicant> _applicantRepo;
        private readonly GenericInterface<WorkTodo> _todoRepo;
        private readonly GenericInterface<WorkImage> _imageRepo;
        private readonly GenericInterface<FeaturedWork> _featuredRepo;
        private readonly KertingDbContext _context;

        public WorkService(
            GenericInterface<Work> workRepo,
            GenericInterface<WorkApplicant> applicantRepo,
            GenericInterface<WorkTodo> todoRepo,
            GenericInterface<WorkImage> imageRepo,
            GenericInterface<FeaturedWork> featuredRepo,
            KertingDbContext context)
        {
            _workRepo = workRepo;
            _applicantRepo = applicantRepo;
            _todoRepo = todoRepo;
            _imageRepo = imageRepo;
            _featuredRepo = featuredRepo;
            _context = context;
        }

        public async Task<IEnumerable<Work>> GetAllOpenWorksAsync()
        {
            return await _context.Work
                .Include(w => w.Author)
                .Include(w => w.Tags).ThenInclude(wt => wt.Tag)
                .Where(w => w.Status == "Open")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PaginatedResponse<WorkListItemDto>> GetVisibleWorksAsync(int userId, int page = 1, int pageSize = 6, WorkFilterParams? filters = null)
        {
            return await GetUserWorkListAsync(userId, page, pageSize, filters, ownOnly: false);
        }

        public async Task<PaginatedResponse<WorkListItemDto>> GetMyWorksAsync(int userId, int page = 1, int pageSize = 6, WorkFilterParams? filters = null)
        {
            return await GetUserWorkListAsync(userId, page, pageSize, filters, ownOnly: true);
        }

        public async Task<PaginatedResponse<Work>> GetAllOpenWorksAsync(int page = 1, int pageSize = 6, WorkFilterParams? filters = null)
        {
            var pagination = new PaginationParams { Page = page, PageSize = pageSize };
            pagination.Validate();

            var query = _context.Work
                .Include(w => w.Author)
                .Include(w => w.Tags).ThenInclude(wt => wt.Tag)
                .AsQueryable();

            // Base filter: Open status
            query = query.Where(w => w.Status == "Open");

            query = ApplyAdvancedFilters(query, filters);

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var skip = (pagination.Page - 1) * pagination.PageSize;
            var items = await query
                .OrderByDescending(w => w.CreatedAtUtc)
                .Skip(skip)
                .Take(pagination.PageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PaginatedResponse<Work>(items, totalCount, pagination.Page, pagination.PageSize);
        }

        private async Task<PaginatedResponse<WorkListItemDto>> GetUserWorkListAsync(int userId, int page, int pageSize, WorkFilterParams? filters, bool ownOnly)
        {
            var pagination = new PaginationParams { Page = page, PageSize = pageSize };
            pagination.Validate();

            var query = _context.Work
                .Include(w => w.Author)
                .Include(w => w.Tags).ThenInclude(wt => wt.Tag)
                .Include(w => w.Applicants)
                .AsQueryable();

            query = ownOnly
                ? query.Where(w => w.AuthorId == userId || w.Applicants.Any(a => a.UserId == userId && a.Status == "Accepted"))
                : query.Where(w => w.Status == "Open" || w.AuthorId == userId || w.Applicants.Any(a => a.UserId == userId && a.Status == "Accepted"));

            query = ApplyAdvancedFilters(query, filters);

            var totalCount = await query.CountAsync();
            var skip = (pagination.Page - 1) * pagination.PageSize;

            var works = await query
                .OrderByDescending(w => w.CreatedAtUtc)
                .Skip(skip)
                .Take(pagination.PageSize)
                .AsNoTracking()
                .ToListAsync();

            var items = works.Select(work => MapToListItem(work, userId)).ToList();

            return new PaginatedResponse<WorkListItemDto>(items, totalCount, pagination.Page, pagination.PageSize);
        }

        private IQueryable<Work> ApplyAdvancedFilters(IQueryable<Work> query, WorkFilterParams? filters)
        {
            if (filters == null)
            {
                return query;
            }

            if (filters.PriceMin.HasValue)
                query = query.Where(w => w.BasePrice >= filters.PriceMin);

            if (filters.PriceMax.HasValue)
                query = query.Where(w => w.BasePrice <= filters.PriceMax);

            if (filters.CreatedFrom.HasValue)
                query = query.Where(w => w.CreatedAtUtc >= filters.CreatedFrom);

            if (filters.CreatedTo.HasValue)
                query = query.Where(w => w.CreatedAtUtc <= filters.CreatedTo);

            if (!string.IsNullOrWhiteSpace(filters.TargetAudience))
                query = query.Where(w => w.TargetAudience == filters.TargetAudience);

            var statusList = filters.GetStatusList();
            if (statusList.Count > 0)
                query = query.Where(w => statusList.Contains(w.Status));

            return query;
        }

        private static WorkListItemDto MapToListItem(Work work, int currentUserId)
        {
            return new WorkListItemDto
            {
                Id = work.Id,
                AuthorId = work.AuthorId,
                Author = work.Author == null
                    ? null
                    : new WorkUserSummaryDto
                    {
                        Id = work.Author.Id,
                        VezetekNev = work.Author.VezetekNev,
                        KeresztNev = work.Author.KeresztNev,
                        Telefon = work.Author.Telefon,
                        Email = work.Author.Email,
                        Telepules = work.Author.Telepules,
                        RoleId = work.Author.RoleId,
                        ImgString = work.Author.IMGString
                    },
                TargetAudience = work.TargetAudience,
                Title = work.Title,
                Description = work.Description,
                BasePrice = work.BasePrice,
                Status = work.Status,
                CreatedAtUtc = work.CreatedAtUtc,
                UpdatedAtUtc = work.UpdatedAtUtc,
                Tags = work.Tags?
                    .Select(tagLink => new WorkTagLinkDto
                    {
                        Tag = tagLink.Tag == null
                            ? null
                            : new WorkTagActivityDto
                            {
                                Activity = tagLink.Tag.Activity
                            }
                    })
                    .ToList() ?? new List<WorkTagLinkDto>(),
                IsCurrentUserRelated = work.AuthorId == currentUserId || (work.Applicants?.Any(a => a.UserId == currentUserId && a.Status == "Accepted") ?? false)
            };
        }

        public async Task<Work> GetWorkByIdAsync(int id)
        {
            try
            {
                var work = await _context.Work
                    .Include(w => w.Author)
                    .Include(w => w.Applicants).ThenInclude(a => a.User)
                    .Include(w => w.Todos)
                    .Include(w => w.Images)
                    .Include(w => w.Tags).ThenInclude(wt => wt.Tag)
                    .FirstOrDefaultAsync(w => w.Id == id);

                if (work != null)
                {
                    work.Cimkek = work.Tags?
                        .Select(wt => wt.Tag?.Activity?.Trim())
                        .Where(activity => !string.IsNullOrWhiteSpace(activity))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }

                return work;
            }
            catch (Exception ex)
            {
                // Fallback: try with minimal includes
                try
                {
                    return await _context.Work
                        .FirstOrDefaultAsync(w => w.Id == id);
                }
                catch
                {
                    return null;
                }
            }
        }

        public async Task<Work> CreateWorkAsync(Work work)
        {
            var user = await _context.User.FindAsync(work.AuthorId);
            if (user == null) throw new InvalidOperationException("Felhasználó nem található.");
            var role = await _context.Role.FindAsync(user.RoleId);

            if (role == null)
            {
                throw new InvalidOperationException("A felhasználó szerepköre nem található.");
            }

            var normalizedRoleName = NormalizeText(role.Name);
            var canCreateWork =
                role.Id == 1 ||
                normalizedRoleName.Contains("kertes") ||
                normalizedRoleName.Contains("gardener");

            if (!canCreateWork)
            {
                throw new UnauthorizedAccessException("Nincs jogosultságod munka kiírására ezzel a szerepkörrel.");
            }

            // Set default values if not provided
            if (string.IsNullOrEmpty(work.Status))
            {
                work.Status = "Open";
            }
            if (work.CreatedAtUtc == default)
            {
                work.CreatedAtUtc = DateTime.UtcNow;
            }

            _context.Work.Add(work);
            await _context.SaveChangesAsync();

            var cimkek = work.Cimkek?
                .Select(cimke => cimke?.Trim())
                .Where(cimke => !string.IsNullOrWhiteSpace(cimke))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (cimkek != null && cimkek.Any())
            {
                foreach (var cimkeNev in cimkek)
                {
                    var tag = await _context.ActivityTag.FirstOrDefaultAsync(t => t.Activity == cimkeNev);
                    if (tag == null)
                    {
                        tag = new Libary.Model.Tag.ActivityTag { Activity = cimkeNev };
                        _context.ActivityTag.Add(tag);
                        await _context.SaveChangesAsync();
                    }

                    _context.WorkTag.Add(new WorkTag { WorkId = work.Id, TagId = tag.Id });
                }
                await _context.SaveChangesAsync();
            }

            return work;
        }

        public async Task<Work> UpdateWorkAsync(int id, Work work)
        {
            var existingWork = await _context.Work.Include(w => w.Tags).FirstOrDefaultAsync(w => w.Id == id);
            if (existingWork != null)
            {
                existingWork.Title = work.Title;
                existingWork.Description = work.Description;
                existingWork.BasePrice = work.BasePrice;
                if (!string.IsNullOrWhiteSpace(work.Status))
                {
                    existingWork.Status = work.Status;
                }
                existingWork.TargetAudience = work.TargetAudience;
                existingWork.UpdatedAtUtc = DateTime.UtcNow;

                _context.WorkTag.RemoveRange(existingWork.Tags);
                
                var cimkek = work.Cimkek?
                    .Select(cimke => cimke?.Trim())
                    .Where(cimke => !string.IsNullOrWhiteSpace(cimke))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (cimkek != null && cimkek.Any())
                {
                    existingWork.Tags = new List<WorkTag>();
                    foreach (var cimkeNev in cimkek)
                    {
                        var tag = await _context.ActivityTag.FirstOrDefaultAsync(t => t.Activity == cimkeNev);
                        if (tag == null)
                        {
                            tag = new Libary.Model.Tag.ActivityTag { Activity = cimkeNev };
                            _context.ActivityTag.Add(tag);
                            await _context.SaveChangesAsync();
                        }
                        
                        existingWork.Tags.Add(new WorkTag { WorkId = existingWork.Id, TagId = tag.Id });
                    }
                }

                _context.Work.Update(existingWork);
                await _context.SaveChangesAsync();
            }
            return existingWork;
        }

        public async Task DeleteWorkAsync(int id)
        {
            var work = await _context.Work.FindAsync(id);
            if (work != null)
            {
                _context.Work.Remove(work);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<WorkApplicant> ApplyForWorkAsync(int workId, int userId, decimal? offeredPrice)
        {
            var work = await _context.Work.FindAsync(workId);
            if (work == null) throw new InvalidOperationException("Munka nem található.");
            if (work.AuthorId == userId) throw new InvalidOperationException("Saját munkádra nem jelentkezhetsz!");

            var user = await _context.User.FindAsync(userId);
            if (user == null) throw new InvalidOperationException("Felhasználó nem található.");
            
            var role = await _context.Role.FindAsync(user.RoleId);

            if (role == null)
            {
                throw new InvalidOperationException("A felhasználó szerepköre nem található.");
            }

            var normalizedRoleName = NormalizeText(role.Name);
            var canApply =
                role.Id == 1 ||
                normalizedRoleName.Contains("kertes") ||
                normalizedRoleName.Contains("gardener") ||
                normalizedRoleName.Contains("hobbi") ||
                normalizedRoleName.Contains("hobby");

            if (!canApply)
            {
                throw new UnauthorizedAccessException("Csak kertészek jelentkezhetnek!");
            }

            var alreadyApplied = await _context.WorkApplicant.AnyAsync(a => a.WorkId == workId && a.UserId == userId);
            if (alreadyApplied) throw new InvalidOperationException("Már jelentkeztél erre a munkára!");

            var applicant = new WorkApplicant
            {
                WorkId = workId,
                UserId = userId,
                OfferedPrice = offeredPrice,
                Status = "Pending"
            };
            _context.WorkApplicant.Add(applicant);
            await _context.SaveChangesAsync();
            return applicant;
        }

        public async Task<IEnumerable<WorkApplicant>> GetWorkApplicantsAsync(int workId)
        {
            return await _context.WorkApplicant
                .Where(a => a.WorkId == workId)
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAtUtc)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<WorkApplicant> AcceptApplicantAsync(int applicantId)
        {
            var applicant = await _context.WorkApplicant
                .Include(a => a.Work)
                .FirstOrDefaultAsync(a => a.Id == applicantId);
                
            if (applicant != null)
            {
                applicant.Status = "Accepted";
                
                // If it's the first accepted person and work is Open, change to InProgress
                if (applicant.Work.Status == "Open")
                {
                    applicant.Work.Status = "InProgress";
                    applicant.Work.UpdatedAtUtc = DateTime.UtcNow;
                }
                
                await _context.SaveChangesAsync();
            }
            return applicant;
        }

        public async Task<WorkTodo> AddTodoAsync(int workId, WorkTodo todo, int userId)
        {
            var work = await _context.Work.Include(w => w.Applicants).FirstOrDefaultAsync(w => w.Id == workId);
            if (work == null) throw new Exception("Munka nem található");

            bool isAuthor = work.AuthorId == userId;
            bool isAcceptedApplicant = work.Applicants.Any(a => a.UserId == userId && a.Status == "Accepted");

            if (!isAuthor && !isAcceptedApplicant)
            {
                throw new Exception("Nincs jogosultságod teendőt hozzáadni ehhez a munkához.");
            }

            todo.WorkId = workId;
            todo.IsDone = false;
            _context.WorkTodo.Add(todo);
            await _context.SaveChangesAsync();
            return todo;
        }

        public async Task<WorkTodo> ToggleTodoAsync(int todoId, int userId, string doneMessage)
        {
            var todo = await _context.WorkTodo
                .Include(t => t.Work)
                .ThenInclude(w => w.Applicants)
                .FirstOrDefaultAsync(t => t.Id == todoId);

            if (todo != null)
            {
                bool isAuthor = todo.Work.AuthorId == userId;
                bool isAcceptedApplicant = todo.Work.Applicants.Any(a => a.UserId == userId && a.Status == "Accepted");

                if (!isAuthor && !isAcceptedApplicant)
                {
                    throw new Exception("Nincs jogosultságod módosítani ezt a feladatot.");
                }

                todo.IsDone = true;
                todo.DoneByUserId = userId;
                todo.DoneMessage = doneMessage;
                await _context.SaveChangesAsync();
            }
            return todo;
        }

        public async Task<WorkImage> UploadWorkImageAsync(int workId, IFormFile image, string directoryPath)
        {
            if (image == null || image.Length == 0) return null;

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var fileExtension = Path.GetExtension(image.FileName);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            var filename = $"{workId}_{timestamp}{fileExtension}";
            var filepath = Path.Combine(directoryPath, filename);

            using (var stream = new FileStream(filepath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var workImage = new WorkImage
            {
                WorkId = workId,
                ImageUrl = filename,
                IsShowcase = false
            };
            
            _context.WorkImage.Add(workImage);
            await _context.SaveChangesAsync();
            return workImage;
        }

        public async Task<bool> ToggleShowcaseImageAsync(int imageId)
        {
            var image = await _context.WorkImage.FindAsync(imageId);
            if (image != null)
            {
                image.IsShowcase = !image.IsShowcase;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<Work> SetWorkStatusAsync(int workId, string status)
        {
            var work = await _context.Work.FindAsync(workId);
            if (work != null)
            {
                work.Status = status;
                work.UpdatedAtUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return work;
        }

        public async Task<IEnumerable<FeaturedWork>> GetFeaturedWorksAsync()
        {
            return await _context.FeaturedWork
                .Include(fw => fw.Work)
                .ThenInclude(w => w.Images)
                .Include(fw => fw.Work.Author)
                .OrderByDescending(fw => fw.FeaturedAtUtc)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<FeaturedWork> FeatureWorkAsync(int workId)
        {
            var work = await _context.Work.FindAsync(workId);
            if (work == null || work.Status != "Public") 
                return null; // Only Public works can be featured

            var existingFeature = await _context.FeaturedWork.FirstOrDefaultAsync(fw => fw.WorkId == workId);
            if (existingFeature != null) return existingFeature;

            var featured = new FeaturedWork { WorkId = workId };
            _context.FeaturedWork.Add(featured);
            await _context.SaveChangesAsync();
            return featured;
        }

        public async Task RemoveFeaturedWorkAsync(int id)
        {
            var featured = await _context.FeaturedWork.FindAsync(id);
            if (featured != null)
            {
                _context.FeaturedWork.Remove(featured);
                await _context.SaveChangesAsync();
            }
        }

        // Phase 4: Bulk Image Upload
        public async Task<IEnumerable<WorkImage>> UploadWorkImagesAsync(int workId, IFormFileCollection files, string directoryPath)
        {
            if (files == null || files.Count == 0)
                throw new InvalidOperationException("No files provided.");

            var work = await _context.Work.FindAsync(workId);
            if (work == null)
                throw new InvalidOperationException("Work not found.");

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var uploadedImages = new List<WorkImage>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var filePath = Path.Combine(directoryPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var image = new WorkImage
                    {
                        WorkId = workId,
                        ImageUrl = $"/resources/Work/{fileName}",
                        IsShowcase = false,
                        UploadedAtUtc = DateTime.UtcNow
                    };

                    _context.WorkImage.Add(image);
                    uploadedImages.Add(image);
                }
            }

            await _context.SaveChangesAsync();
            return uploadedImages;
        }

        // Phase 4: Delete Image
        public async Task DeleteWorkImageAsync(int imageId, int userId)
        {
            var image = await _context.WorkImage
                .Include(i => i.Work)
                .FirstOrDefaultAsync(i => i.Id == imageId);

            if (image == null)
                throw new InvalidOperationException("Image not found.");

            if (image.Work?.AuthorId != userId)
                throw new UnauthorizedAccessException("Only the work author can delete images.");

            // Delete file from disk
            if (!string.IsNullOrEmpty(image.ImageUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Work", Path.GetFileName(image.ImageUrl));
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }

            _context.WorkImage.Remove(image);
            await _context.SaveChangesAsync();
        }

        // Phase 4: Update Image Metadata (showcase, pairing)
        public async Task<WorkImage> UpdateImageMetadataAsync(int imageId, WorkImage metadata)
        {
            var image = await _context.WorkImage.FindAsync(imageId);
            if (image == null)
                throw new InvalidOperationException("Image not found.");

            image.IsShowcase = metadata.IsShowcase;
            if (metadata.RelatedImageId.HasValue)
                image.RelatedImageId = metadata.RelatedImageId.Value;

            await _context.SaveChangesAsync();
            return image;
        }

        // Phase 4: Link Image Pair (before/after)
        public async Task<bool> LinkImagePairAsync(int imageId, int relatedImageId)
        {
            var image = await _context.WorkImage.FindAsync(imageId);
            var relatedImage = await _context.WorkImage.FindAsync(relatedImageId);

            if (image == null || relatedImage == null)
                throw new InvalidOperationException("One or both images not found.");

            if (image.WorkId != relatedImage.WorkId)
                throw new InvalidOperationException("Both images must belong to the same work.");

            // Create bidirectional link
            image.RelatedImageId = relatedImageId;
            relatedImage.RelatedImageId = imageId;

            await _context.SaveChangesAsync();
            return true;
        }

        // Phase 6: Reject Applicant
        public async Task<WorkApplicant> RejectApplicantAsync(int applicantId, int userId)
        {
            var applicant = await _context.WorkApplicant
                .Include(a => a.Work)
                .FirstOrDefaultAsync(a => a.Id == applicantId);

            if (applicant == null)
                throw new InvalidOperationException("Applicant not found.");

            if (applicant.Work?.AuthorId != userId)
                throw new UnauthorizedAccessException("Only the work author can reject applicants.");

            if (applicant.Status != "Pending")
                throw new InvalidOperationException("Can only reject pending applicants.");

            applicant.Status = "Rejected";
            await _context.SaveChangesAsync();
            return applicant;
        }

        // Phase 6: Withdraw Application
        public async Task<WorkApplicant> WithdrawApplicationAsync(int applicantId, int userId)
        {
            var applicant = await _context.WorkApplicant
                .FirstOrDefaultAsync(a => a.Id == applicantId);

            if (applicant == null)
                throw new InvalidOperationException("Applicant not found.");

            if (applicant.UserId != userId)
                throw new UnauthorizedAccessException("You can only withdraw your own applications.");

            if (applicant.Status != "Pending")
                throw new InvalidOperationException("Can only withdraw pending applications.");

            applicant.Status = "Withdrawn";
            await _context.SaveChangesAsync();
            return applicant;
        }

        private static string NormalizeText(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var normalized = input.Normalize(NormalizationForm.FormD);
            var filteredChars = normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
            return new string(filteredChars.ToArray()).ToLowerInvariant();
        }
    }
}