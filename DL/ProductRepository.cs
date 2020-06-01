using Authorization.DL.Products;
using Authorization.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace Authorization.DL
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
    }
}
