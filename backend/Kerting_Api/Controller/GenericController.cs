using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kerting_Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Általános generic CRUD controller.
    /// Könnyű újrafelhasználhatóságot ad egyszerű entitásokhoz.
    /// </summary>
    public class GenericController<T> : ControllerBase where T : class
    {
        private readonly Interface.GenericInterface<T> _genericService;
        public GenericController(Interface.GenericInterface<T> genericService)
        {
            _genericService = genericService;
        }

        /// <summary>
        /// Összes rekord listázása.
        /// </summary>
        [HttpGet]
        public async Task<List<T>> GetAll() => await _genericService.GetAll();

        /// <summary>
        /// Egy rekord lekérése ID alapján.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            var entity = await _genericService.GetById(id);
            return entity is null ? NotFound() : Ok(entity);
        }

        /// <summary>
        /// Új rekord létrehozása.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] T entity)
        {
            await _genericService.Add(entity);
            return Ok();
        }

        /// <summary>
        /// Meglévő rekord frissítése.
        /// </summary>
        [HttpPut]
        public async Task<ActionResult> Update([FromBody] T entity)
        {
            await _genericService.update(entity);
            return Ok();
        }

        /// <summary>
        /// Rekord törlése ID alapján.
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            await _genericService.Delete(id);
            return Ok();
        }
    }
}
