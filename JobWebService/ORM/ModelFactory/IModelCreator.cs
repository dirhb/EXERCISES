using System.Data;

namespace JobWebService.ORM.ModelFactory
{
    public interface IModelCreator<T>
    {
        T CreateModel(IDataReader reader);
    }
}
