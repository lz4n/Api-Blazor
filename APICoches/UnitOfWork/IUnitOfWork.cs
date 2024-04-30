using APICoches.CarsRepository;
using APICoches.Models;
using Microsoft.EntityFrameworkCore;

namespace APICoches.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepositoryBase<Car> _carRepository { get; }
        IRepositoryBase<Sale> _saleRepository { get; }
        int Save();
    }
}
