using APICoches.Infraestructure;
using APICoches.Models;

namespace APICoches.CarsRepository
{
    public class CarRepository : IRepositoryBase<Car>
    {
        private AppDbContext _context { get; set; }

        public CarRepository(AppDbContext context)
        {
            _context = context;
        }

        public bool Create(Car obj)
        {
            try
            {
                _context.Cars.Add(obj);
            } catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool Delete(Car obj)
        {
            try
            {
                _context.Cars.Remove(obj);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public List<Car> GetAll()
        {
            return _context.Cars.ToList();
        }

        public List<Car> GetByPredicate(Predicate<Car> predicate)
        {
            return _context.Cars.ToList().FindAll(predicate);
        }

        public bool Update(Car obj)
        {
            try
            {
                _context.Cars.Update(obj);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public Car GetById(int id)
        {
            return _context.Cars.Find(id);
        }
    }
}
