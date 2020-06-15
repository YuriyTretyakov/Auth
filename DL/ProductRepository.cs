using ColibriWebApi.DL;
using ColibriWebApi.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace ColibriWebApi.DL
{
    public class ProductRepository
    {
        private readonly AuthDbContext _context;

        public ProductRepository(AuthDbContext context)
        {
            _context = context;
        }

        public Product[] GetAllProducts()
        {
            return _context.Products.ToArray();
        }

        public Product[] GetAllActiveProducts()
        {
            return _context.Products.Where(p=>p.IsActive==true).ToArray();
        }

        public Product GetProductById(int id)
        {
            return _context.Products.FirstOrDefault(p => p.Id == id);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return ((await _context.SaveChangesAsync()) > 0);
        }

        public async Task UpdateProduct(Product product,int id)
        {
            var prod = _context.Products.FirstOrDefault(p => p.Id == id);

            prod.IsActive = product.IsActive;

            prod.Picture = product.Picture;
            prod.Price = product.Price;
            prod.Description = product.Description;
            prod.Duration = product.Duration;
            prod.Title = product.Title;

            await SaveChangesAsync();
        }

        public async Task CreateProduct(Product product)
        {
            _context.Products.Add(product);
            await SaveChangesAsync();
        }
    }
}
