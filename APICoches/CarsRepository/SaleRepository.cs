using APICoches.Infraestructure;
using APICoches.Models;
using Microsoft.EntityFrameworkCore;

namespace APICoches.CarsRepository
{
    public class SaleRepository : IRepositoryBase<Sale>
    {
        private AppDbContext _context { get; set; }

        public SaleRepository(AppDbContext context)
        {
            _context = context;
            _context.Sales.Include(sale => sale.Car);
        }

        public bool Create(Sale obj)
        {
            try
            {
                _context.Sales.Add(obj);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool Delete(Sale obj)
        {
            try
            {
                _context.Sales.Remove(obj);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public List<Sale> GetAll()
        {
            return _context.Sales.ToList();
        }

        public List<Sale> GetByPredicate(Predicate<Sale> predicate)
        {
            return _context.Sales.ToList().FindAll(predicate);
        }

        public bool Update(Sale obj)
        {
            try
            {
                _context.Sales.Update(obj);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public Sale GetById(int id)
        {
            return _context.Sales.Find(id);
        }
    }
}
