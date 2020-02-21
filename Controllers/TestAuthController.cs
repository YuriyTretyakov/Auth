using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.Controllers
{
    [Route("Test")]
    public class TestAuthController : Controller
    {
        [Authorize]
        [HttpGet("Get")]
        public IActionResult Index()
        {
            return Ok("You are authorized to see sthis shit");
        }
    }
}