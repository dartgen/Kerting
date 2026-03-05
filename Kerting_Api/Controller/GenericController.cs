using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kerting_Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenericController<T> : ControllerBase where T : class
    {
        private readonly Interface.GenericInterface<T> _genericService;
        public GenericController(Interface.GenericInterface<T> genericService)
        {
            _genericService = genericService;
        }

        [HttpGet]
        public async Task<List<T>> GetAll() => await _genericService.GetAll();

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            var entity = await _genericService.GetById(id);
            return entity is null ? NotFound() : Ok(entity);
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] T entity)
        {
            await _genericService.Add(entity);
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> Update([FromBody] T entity)
        {
            await _genericService.update(entity);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            await _genericService.Delete(id);
            return Ok();
        }
    }
}
