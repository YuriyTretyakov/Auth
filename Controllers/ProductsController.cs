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

        [HttpGet("all")]
        public IActionResult GetAllProducts()
        {
            return Ok(_ds.GetAllProducts());
        }

        [HttpGet("GetProduct/{id}")]
        public IActionResult GetProductById(string id)
        {
            if (string.IsNullOrWhiteSpace(id) ||
                    !int.TryParse(id, out int numId))
                return BadRequest($"Invalid Id: '{id}'");

            return Ok(_ds.GetProductById(numId));
        }
    }
}