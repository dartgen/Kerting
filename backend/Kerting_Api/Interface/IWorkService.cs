using Kerting_Api.DTO;
using Libary.Model.Work;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kerting_Api.Interface
{
    public interface IWorkService
    {
        // Legacy - kept for backward compatibility
        Task<IEnumerable<Work>> GetAllOpenWorksAsync();

        // New pagination/filtering
        Task<PaginatedResponse<Work>> GetAllOpenWorksAsync(int page = 1, int pageSize = 6, WorkFilterParams? filters = null);
        Task<PaginatedResponse<WorkListItemDto>> GetVisibleWorksAsync(int userId, int page = 1, int pageSize = 6, WorkFilterParams? filters = null);
        Task<PaginatedResponse<WorkListItemDto>> GetMyWorksAsync(int userId, int page = 1, int pageSize = 6, WorkFilterParams? filters = null);
        
        Task<Work> GetWorkByIdAsync(int id);
        Task<Work> CreateWorkAsync(Work work);
        Task<Work> UpdateWorkAsync(int id, Work work);
        Task DeleteWorkAsync(int id);

        Task<WorkApplicant> ApplyForWorkAsync(int workId, int userId, decimal? offeredPrice);
        Task<IEnumerable<WorkApplicant>> GetWorkApplicantsAsync(int workId);
        Task<WorkApplicant> AcceptApplicantAsync(int applicantId);
        Task<WorkApplicant> RejectApplicantAsync(int applicantId, int userId);
        Task<WorkApplicant> WithdrawApplicationAsync(int applicantId, int userId);
        
        Task<WorkTodo> AddTodoAsync(int workId, WorkTodo todo, int userId);
        Task<WorkTodo> ToggleTodoAsync(int todoId, int userId, string doneMessage);
        
        Task<WorkImage> UploadWorkImageAsync(int workId, IFormFile image, string directoryPath);
        Task<IEnumerable<WorkImage>> UploadWorkImagesAsync(int workId, IFormFileCollection files, string directoryPath);
        Task<bool> ToggleShowcaseImageAsync(int imageId);
        Task DeleteWorkImageAsync(int imageId, int userId);
        Task<WorkImage> UpdateImageMetadataAsync(int imageId, WorkImage metadata);
        Task<bool> LinkImagePairAsync(int imageId, int relatedImageId);

        Task<Work> SetWorkStatusAsync(int workId, string status);
        
        Task<IEnumerable<FeaturedWork>> GetFeaturedWorksAsync();
        Task<FeaturedWork> FeatureWorkAsync(int workId);
        Task RemoveFeaturedWorkAsync(int id);
    }
}