using Authorization.DL;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.Controllers
{

    [Route("products")]
    public class ProductsController : Controller
    {
        private readonly ProductRepository _ds;

        public ProductsController(ProductRepository ds)
        {
            _ds = ds;
        }

        [HttpGet("products")]
        public IActionResult GetAllProducts()
        {
            return Ok(_ds.GetAllProducts());
        }
    }
}