using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Authorization.Controllers
{
    [Route("Test")]
    public class TestAuthController : Controller
    {
        [Authorize]
        [HttpGet("Get")]
        public IActionResult Index()
        {
            return Ok(new { data = $"You are authorized to see sthis shit {Guid.NewGuid()}" });
        }
    }
}