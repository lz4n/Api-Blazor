
using APICoches.CarsRepository;
using APICoches.Infraestructure;
using APICoches.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace APICoches.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IRepositoryBase<Car> _carRepository { get; }
        public IRepositoryBase<Sale> _saleRepository { get; }

        public UnitOfWork(AppDbContext context, IRepositoryBase<Car> carRepository, IRepositoryBase<Sale> saleRepository)
        {
            _context = context;
            _carRepository = carRepository;
            _saleRepository = saleRepository;
        }

        public int Save()
        {
            return _context.SaveChanges();
        }


        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _context.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
