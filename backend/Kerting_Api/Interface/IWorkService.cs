using Kerting_Api.DTO;
using Libary.Model.Work;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kerting_Api.Interface
{
    /// <summary>
    /// Work modul üzleti szerződése.
    /// Lefedi a munkák listázását, jelentkezéseket, teendőket, képkezelési műveleteket
    /// és kiemelési (featured) funkciókat is.
    /// </summary>
    public interface IWorkService
    {
        /// <summary>
        /// Ellenőrzi, hogy a megadott user admin szerepkörben van-e.
        /// </summary>
        Task<bool> IsAdminAsync(int userId);

        // Régi jellegű metódus, visszafelé kompatibilitás miatt megtartva.
        /// <summary>
        /// Visszafelé kompatibilis nyitott munka lista.
        /// </summary>
        Task<IEnumerable<Work>> GetAllOpenWorksAsync();

        // Új lapozás/szűrés.
        /// <summary>
        /// Nyitott munkák lapozott és szűrhető listája.
        /// </summary>
        Task<PaginatedResponse<Work>> GetAllOpenWorksAsync(int page = 1, int pageSize = 6, WorkFilterParams? filters = null);

        /// <summary>
        /// Az aktuális user számára látható munkák (nyitott + saját + elfogadott jelentkezések) listája.
        /// </summary>
        Task<PaginatedResponse<WorkListItemDto>> GetVisibleWorksAsync(int userId, int page = 1, int pageSize = 6, WorkFilterParams? filters = null);

        /// <summary>
        /// A user saját releváns munkái (szerző vagy elfogadott jelentkező) listázása.
        /// </summary>
        Task<PaginatedResponse<WorkListItemDto>> GetMyWorksAsync(int userId, int page = 1, int pageSize = 6, WorkFilterParams? filters = null);

        /// <summary>
        /// Admin felületre szánt publikus munkák listája.
        /// </summary>
        Task<IEnumerable<Work>> GetAdminPublicWorksAsync();
        
        /// <summary>
        /// Egy munka teljes részletes adatait kérdezi le azonosító alapján.
        /// </summary>
        Task<Work> GetWorkByIdAsync(int id);

        /// <summary>
        /// Új munka létrehozása jogosultság és adattisztítás ellenőrzéssel.
        /// </summary>
        Task<Work> CreateWorkAsync(Work work);

        /// <summary>
        /// Meglévő munka frissítése (mezők + címkék).
        /// </summary>
        Task<Work> UpdateWorkAsync(int id, Work work);

        /// <summary>
        /// Munka törlése azonosító alapján.
        /// </summary>
        Task DeleteWorkAsync(int id);

        /// <summary>
        /// Jelentkezés küldése munkára opcionális ajánlati árral.
        /// </summary>
        Task<WorkApplicant> ApplyForWorkAsync(int workId, int userId, decimal? offeredPrice);

        /// <summary>
        /// Jelentkezők listája a megadott munkára.
        /// </summary>
        Task<IEnumerable<WorkApplicant>> GetWorkApplicantsAsync(int workId);

        /// <summary>
        /// Jelentkező elfogadása.
        /// </summary>
        Task<WorkApplicant> AcceptApplicantAsync(int applicantId);

        /// <summary>
        /// Jelentkező elutasítása (általában szerzői jogosultság mellett).
        /// </summary>
        Task<WorkApplicant> RejectApplicantAsync(int applicantId, int userId);

        /// <summary>
        /// Saját jelentkezés visszavonása.
        /// </summary>
        Task<WorkApplicant> WithdrawApplicationAsync(int applicantId, int userId);
        
        /// <summary>
        /// Új teendő elem felvétele a munkához.
        /// </summary>
        Task<WorkTodo> AddTodoAsync(int workId, WorkTodo todo, int userId);

        /// <summary>
        /// Teendő állapotának végleges teljesítettre állítása üzenettel.
        /// </summary>
        Task<WorkTodo> ToggleTodoAsync(int todoId, int userId, string doneMessage);
        
        /// <summary>
        /// Egy kép feltöltése a munkához.
        /// </summary>
        Task<WorkImage> UploadWorkImageAsync(int workId, IFormFile image, string directoryPath);

        /// <summary>
        /// Bulk képfeltöltés.
        /// </summary>
        Task<IEnumerable<WorkImage>> UploadWorkImagesAsync(int workId, IFormFileCollection files, string directoryPath);

        /// <summary>
        /// Kiemelt (showcase) flag váltása egy képen.
        /// </summary>
        Task<bool> ToggleShowcaseImageAsync(int imageId);

        /// <summary>
        /// Kép törlése jogosultság-ellenőrzéssel.
        /// </summary>
        Task DeleteWorkImageAsync(int imageId, int userId);

        /// <summary>
        /// Kép metadata frissítése (pl. showcase vagy párosítási mezők).
        /// </summary>
        Task<WorkImage> UpdateImageMetadataAsync(int imageId, WorkImage metadata);

        /// <summary>
        /// Before/after jellegű képpár összekötés két kép között.
        /// </summary>
        Task<bool> LinkImagePairAsync(int imageId, int relatedImageId);

        /// <summary>
        /// Munka státusz explicit állítása.
        /// </summary>
        Task<Work> SetWorkStatusAsync(int workId, string status);
        
        /// <summary>
        /// Kiemelt munkák listája carousel/landing nézethez.
        /// </summary>
        Task<IEnumerable<FeaturedWork>> GetFeaturedWorksAsync();

        /// <summary>
        /// Munka kiemelése, ha megfelel az üzleti feltételeknek.
        /// </summary>
        Task<FeaturedWork> FeatureWorkAsync(int workId);

        /// <summary>
        /// Kiemelés törlése.
        /// </summary>
        Task RemoveFeaturedWorkAsync(int id);
    }
}
