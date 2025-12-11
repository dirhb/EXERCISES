namespace JobWebService.ORM.Repositories
{
    public interface IRepository<T>
    {
        bool Create(T model);
        bool Update(T model);
        bool Delete(string id);
        List<T> GetALL();
        T GetById(string id);
    }
}
