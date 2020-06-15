using ColibriWebApi.DL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
namespace ColibriWebApi.Controllers
{

    [Route("products")]
    public class ProductsController : Controller
    {
        private readonly ProductRepository _ds;
        private readonly IHostingEnvironment _hostingEnvironment;

        public ProductsController(ProductRepository ds, IHostingEnvironment hostingEnvironment)
        {
            _ds = ds;
            _hostingEnvironment = hostingEnvironment;
        }

     //   [Authorize(Roles = "Administrator")]
        [HttpGet("getallforadmin")]
        public IActionResult GetAllProducts()
        {
            return Ok(_ds.GetAllProducts());
        }

        [HttpGet("all")]
        public IActionResult GetAllActiveProducts()
        {
            return Ok(_ds.GetAllActiveProducts());
        }

        [HttpGet("GetProduct/{id}")]
        public IActionResult GetProductById(string id)
        {
            if (string.IsNullOrWhiteSpace(id) ||
                    !int.TryParse(id, out int numId))
                return BadRequest($"Invalid Id: '{id}'");

            return Ok(_ds.GetProductById(numId));
        }

        
        [HttpPut("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProductById([FromBody]Product updateModel, int id)
        {
            if (id==0) 
                 return BadRequest($"Invalid Id: '{id}'");

            await _ds.UpdateProduct(updateModel, id);
            return Ok();
        }

        [Authorize]
        [HttpPost("UploadProductPicture"), DisableRequestSizeLimit]
        public async Task<ActionResult> UploadFileAsync()
        {
            try
            {
                var file = Request.Form.Files[0];

                if (string.IsNullOrWhiteSpace(_hostingEnvironment.WebRootPath))
                {
                    _hostingEnvironment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                string webRootPath = _hostingEnvironment.WebRootPath;
                string newPath = Path.Combine(webRootPath, "ProductImgs");
                string fullPath = "";
                string fileName = "";
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }
                if (file.Length > 0)
                {
                    fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    fullPath = Path.Combine(newPath, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }


                var serverAdress = $"{Request.Scheme}://{Request.Host}/ProductImgs/{fileName}";

                return Ok(Json(serverAdress));
            }
            catch (System.Exception ex)
            {
                return BadRequest(Json("Upload Failed: " + ex.Message));
            }
        }

        [Authorize]
        [HttpPost("Add")]
        public async Task<ActionResult> Add([FromBody] Product product)
        {
            await _ds.CreateProduct(product);
            return Ok();
        }
    }
}