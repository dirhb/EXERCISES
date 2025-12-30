using Microsoft.AspNetCore.Mvc;
using JobModels;
using JobWebService.ORM.Repositories;

namespace JobWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IRepository<Job> _repo;

        public JobController(IRepository<Job> repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var items = _repo.ReadAll();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var item = _repo.Read(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Job model)
        {
            if (model == null) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ok = _repo.Insert(model);
            if (!ok) return StatusCode(500, "Failed to create job");
            return CreatedAtAction(nameof(Get), new { id = model.JobID }, model);
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, [FromBody] Job model)
        {
            if (model == null || id != model.JobID) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ok = _repo.Update(model);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var ok = _repo.Delete(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        // optional: paging
        [HttpGet("paged")]
        public IActionResult GetPaged([FromQuery] int page = 1, [FromQuery] int size = 25)
        {
            if (page < 1 || size < 1) return BadRequest("Invalid paging parameters");
            // repository does not expose a paged method directly on interface; try casting
            if (_repo is JobWebService.ORM.Repositories.JobRepository jr)
            {
                var items = jr.GetPaged(page, size);
                return Ok(items);
            }
            return BadRequest("Paged query is not supported by this repository");
        }
    }
}
