using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
}
