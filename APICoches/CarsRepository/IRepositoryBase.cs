namespace APICoches.CarsRepository
{
    public interface IRepositoryBase<T>
    {
        List<T> GetAll();
        List<T> GetByPredicate(Predicate<T> predicate);
        T GetById(int id);
        bool Create(T obj);
        bool Update(T obj);
        bool Delete(T obj);
    }
}
