using Microsoft.AspNetCore.Mvc;
using JobModels;
using JobWebService.ORM.Repositories;

namespace JobWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IRepository<User> _repo;
        private readonly UserRepository _userRepo; // used for credential lookup

        public UserController(IRepository<User> repo, UserRepository userRepo)
        {
            _repo = repo;
            _userRepo = userRepo;
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
        public IActionResult Create([FromBody] User model)
        {
            if (model == null) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ok = _repo.Insert(model);
            if (!ok) return StatusCode(500, "Failed to create user");
            return CreatedAtAction(nameof(Get), new { id = model.UserID }, model);
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, [FromBody] User model)
        {
            if (model == null || id != model.UserID) return BadRequest();
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

        public record LoginRequest(string Email, string Password);

        // Simple login endpoint (demo only). Do NOT use plaintext passwords in production.
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            if (req == null) return BadRequest();
            var user = _userRepo.GetByCredentials(req.Email, req.Password);
            if (user == null) return Unauthorized();
            return Ok(user);
        }
    }
}
